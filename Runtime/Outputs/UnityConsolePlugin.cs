using System;
using System.Text;
using System.Reflection;
using Unity.Profiling;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;
using WhiteSparrow.Shared.Logging.Inputs;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public class UnityConsolePlugin : AbstractChirpOutput, IChirpInput, ILogHandler
    {
        /// <summary>
        /// When true, this output defers per-log stack traces to Unity's native capture instead of
        /// printing Chirp's reconstructed trace: a normal log that asked for a trace is emitted with
        /// <see cref="LogOption.None"/> so Unity captures it — the pipeline's [HideInCallstack]
        /// frames resolve that back to the real call site — and an exception is handed to Unity's
        /// exception logger. When false, Unity is told to add nothing (<see cref="LogOption.NoStacktrace"/>)
        /// and Chirp's own filtered, async-resolved trace is appended as text instead.
        ///
        /// Defaults to true in the editor and false in player builds. Native capture gives correct
        /// file/line and IDE double-click in the editor, but is unreliable across async/UniTask
        /// frames — which is why builds keep Chirp's reconstructed trace. Flip this to false in the
        /// editor to preview the build's behaviour.
        /// </summary>
        public static bool EditorStackTraces =
#if UNITY_EDITOR
            true;
#else
            false;
#endif

        private static ILogHandler m_DefaultUnityLogHandler;


        private IChirpReceiver m_Receiver;
        private ChirpLogger m_Channel;

        public UnityConsolePlugin()
        {
            if(m_DefaultUnityLogHandler == null)
                m_DefaultUnityLogHandler = Debug.unityLogger.logHandler;
        }

        void IChirpInput.InitializeInput(IChirpReceiver receiver)
        {
            m_Receiver = receiver;
            m_Channel = new ChirpLogger("Unity");

            // Suppression of Unity's own stack trace is per-call (LogOption.NoStacktrace) rather
            // than global — Chirp deliberately leaves Application.SetStackTraceLogType alone so a
            // project's Stack Trace Logging settings survive Chirp being installed.
            ProbeInternalLog();

            Debug.unityLogger.logHandler = this;
        }
        
        protected override void OnInitialize()
        {
        }

        protected override bool Filter(ChirpLog logEvent)
        {
            return true;
        }

        // Reused single-element args buffer for the "{0}" LogFormat call below. Unity's handler
        // consumes it synchronously; ThreadStatic because logs can be submitted from any thread.
        [ThreadStatic]
        private static object[] s_LogFormatArgs;

        // The Process marker also covers the hand-off into Unity's log handler, so the
        // difference between it and FormatConsoleLog is Unity's own per-log cost.
        private static readonly ProfilerMarker s_ProcessMarker = new ProfilerMarker("UnityConsolePlugin.Process");
        private static readonly ProfilerMarker s_FormatConsoleLogMarker = new ProfilerMarker("UnityConsolePlugin.FormatConsoleLog");

        [HideInCallstack]
        protected override void Process(ChirpLog logEvent)
        {
            using var _ = s_ProcessMarker.Auto();

#if UNITY_EDITOR
            // Editor + EditorStackTraces: render the exception through Unity's own logger so the
            // Console links it to the throw site, triggers error-pause, and so on. Prefer the
            // per-call internal method; fall back to the default handler's public LogException when
            // it is the only one available. When Internal_Log bound but Internal_LogException did
            // not, drop through to the text path below, which still carries the exception's trace.
            var exception = logEvent.Exception;
            if (EditorStackTraces && exception != null)
            {
                if (s_InternalLogException != null)
                {
                    s_InternalLogException(exception, logEvent.Context);
                    return;
                }
                if (s_InternalLog == null)
                {
                    m_DefaultUnityLogHandler.LogException(exception, logEvent.Context);
                    return;
                }
            }
#endif

            if (s_InternalLog != null)
            {
                // Default (builds, or EditorStackTraces off): Unity adds no trace of its own, and
                // Chirp's reconstructed trace — filtered by [HideInCallstack], resolved across
                // async/UniTask frames — is appended as text.
                LogOption logOption = LogOption.NoStacktrace;
                bool appendChirpStackTrace = true;

                // Editor: for ordinary logs, hand the trace off to Unity's native capture. With a
                // trace requested that is LogOption.None (the pipeline's [HideInCallstack] frames
                // land it on the real call site); without one it stays suppressed. Chirp's own
                // trace is not appended. Exceptions are excluded — they were handled natively above
                // when possible, and otherwise want their throw-site trace, which the text path
                // below preserves.
                if (EditorStackTraces && logEvent.Level != LogLevel.Exception)
                {
                    logOption = logEvent.Options.AddStackTrace ? LogOption.None : LogOption.NoStacktrace;
                    appendChirpStackTrace = false;
                }

                s_InternalLog(UnityLogUtil.ToUnityLogType(logEvent.Level), logOption, FormatConsoleLog(logEvent, appendChirpStackTrace), logEvent.Context);
                return;
            }

            // Fallback: the public LogFormat has no per-call LogOption, so Unity appends its own
            // trace according to the project's settings. Appending Chirp's as well would double it,
            // so Unity's is left to stand alone.
            var args = s_LogFormatArgs ??= new object[1];
            args[0] = FormatConsoleLog(logEvent, false);
            m_DefaultUnityLogHandler.LogFormat(UnityLogUtil.ToUnityLogType(logEvent.Level), logEvent.Context, "{0}", args);
            args[0] = null;
        }

        // Binds Unity's internal DebugLogHandler.Internal_Log. Two reasons: it takes a per-call
        // LogOption, which is what lets Chirp suppress Unity's stack trace without touching the
        // project-wide Application.SetStackTraceLogType settings; and it skips the
        // string.Format("{0}", ...) full-message copy the public ILogHandler API performs on
        // every log. Falls back to LogFormat whenever the internal method is missing or its
        // signature changed — see Process for what that costs.
        // Only the LogOption-carrying overload is bound. The pre-2017 three-argument Internal_Log
        // cannot express NoStacktrace, so binding it would silently print both Unity's trace and
        // Chirp's; that case is left to the fallback path instead.
        private delegate void InternalLogDelegate(LogType level, LogOption options, string msg, Object obj);
        private static InternalLogDelegate s_InternalLog;

#if UNITY_EDITOR
        // Unity's native exception logger, editor-only (see EditorStackTraces). Takes only the
        // exception and the context object — it derives the trace from the exception itself, so
        // there is no message or trace parameter to pass. Bound alongside Internal_Log; when it is
        // missing, exceptions degrade to the formatted-text path, which still carries the trace.
        private delegate void InternalLogExceptionDelegate(Exception exception, Object obj);
        private static InternalLogExceptionDelegate s_InternalLogException;
#endif

        private static bool s_InternalLogProbed;

        private static void ProbeInternalLog()
        {
            if (s_InternalLogProbed)
                return;
            s_InternalLogProbed = true;

            try
            {
                var handlerType = typeof(Debug).Assembly.GetType("UnityEngine.DebugLogHandler");
                if (handlerType == null)
                    return;

                const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

                var method = handlerType.GetMethod("Internal_Log", flags, null,
                    new[] { typeof(LogType), typeof(LogOption), typeof(string), typeof(Object) }, null);
                if (method == null)
                    return;

                s_InternalLog = (InternalLogDelegate)Delegate.CreateDelegate(typeof(InternalLogDelegate), method, false);

#if UNITY_EDITOR
                // Optional, editor-only: exceptions still work without it (formatted-text path),
                // so a bind failure here is not fatal to the fast path.
                var exceptionMethod = handlerType.GetMethod("Internal_LogException", flags, null,
                    new[] { typeof(Exception), typeof(Object) }, null);
                if (exceptionMethod != null)
                    s_InternalLogException = (InternalLogExceptionDelegate)Delegate.CreateDelegate(typeof(InternalLogExceptionDelegate), exceptionMethod, false);
#endif
            }
            catch (Exception)
            {
                s_InternalLog = null;
#if UNITY_EDITOR
                s_InternalLogException = null;
#endif
            }
        }

        protected override void OnDispose()
        {
            if (Debug.unityLogger.logHandler == this)
                Debug.unityLogger.logHandler = m_DefaultUnityLogHandler;
            m_DefaultUnityLogHandler = null;
        }

        // Sized to hold prefix + message + a typical stack trace in a single chunk. Clear() only
        // takes its allocation-free fast path while the builder is still one chunk — once it has
        // spilled, every Clear() allocates to collapse the chain back down, on every log. Same
        // reasoning as k_StackTraceBuilderCapacity in LoggingStackTraceUtil, with room for the
        // trace this builder appends on top of what that one produced.
        private const int k_FormatBuilderCapacity = 4096;

        [ThreadStatic]
        private static StringBuilder s_HelperFormatBuilder;

        // internal rather than private so the allocation tests can budget the formatting step on
        // its own. Measuring it through Process would fold in Unity's console retention and the
        // test framework's own per-log capture, neither of which Chirp controls.
        internal string FormatConsoleLog(ChirpLog logEvent, bool appendStackTrace)
        {
            using var _ = s_FormatConsoleLogMarker.Auto();

            if (s_HelperFormatBuilder == null)
                s_HelperFormatBuilder = new StringBuilder(k_FormatBuilderCapacity);
            else
                s_HelperFormatBuilder.Clear();

            if (logEvent.Source is { UseChannel: true } source)
            {
                s_HelperFormatBuilder.Append(source.ChannelPrefix);
            }

            if (logEvent.Options.HasMarkdown)
            {
                s_HelperFormatBuilder.Append(LoggingMarkdownUtil.Parse(logEvent.Message, logEvent.Source.Style));
            }
            else
            {
                s_HelperFormatBuilder.Append(logEvent.Message);
            }
            
            if (appendStackTrace && !string.IsNullOrWhiteSpace(logEvent.StackTrace))
            {
                s_HelperFormatBuilder.AppendLine();
                s_HelperFormatBuilder.Append(logEvent.StackTrace);
            }

            return s_HelperFormatBuilder.ToString();
        }


        [HideInCallstack]
        void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            // Debug.Log(object) always arrives as ("{0}", message) — skip the string.Format copy.
            string message;
            if (args is { Length: 1 } && format == "{0}")
            {
                var arg = args[0];
                message = arg as string ?? arg?.ToString() ?? string.Empty;
            }
            else
            {
                message = string.Format(format, args);
            }

            var log = ChirpLogUtil.ConstructLog(message, context);
            log.Level = UnityLogUtil.FromUnityLogType(logType);
            log.Source = m_Channel;
            if (log.Level >= LogLevel.Assert)
            {
                log.StackTrace = LoggingStackTraceUtil.ExtractStackTrace();
            }
            m_Receiver.Submit(log);
        }

        [HideInCallstack]
        void ILogHandler.LogException(Exception exception, Object context)
        {
            var log = ChirpLogUtil.ConstructLog(exception.Message, context);
            log.Source = m_Channel;
#if UNITY_EDITOR
            // Editor-only: lets the console render the exception natively. StackTrace below is
            // always populated so text outputs (and player builds) still carry the trace.
            log.Exception = exception;
#endif
            log.StackTrace = StackTraceUtility.ExtractStringFromException(exception);
            log.Level = LogLevel.Exception;
            m_Receiver.Submit(log);
        }
    }
    
    public static class UnityLogUtil
    {
        public static LogLevel FromUnityLogType(LogType logType)
        {
            switch (logType)
            {
                case LogType.Assert:
                    return LogLevel.Assert;
                case LogType.Error:
                    return LogLevel.Error;
                case LogType.Exception:
                    return LogLevel.Exception;
                case LogType.Warning:
                    return LogLevel.Warning;
                default:
                    return LogLevel.Debug;
            }
        }

        public static LogType ToUnityLogType(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Assert:
                    return LogType.Assert;
                case LogLevel.Debug:
                    return LogType.Log;
                case LogLevel.Error:
                    return LogType.Error;
                case LogLevel.Exception:
                    return LogType.Exception;
                case LogLevel.Warning:
                    return LogType.Warning;
                default:
                    return LogType.Log;
            }
        }
    }
}