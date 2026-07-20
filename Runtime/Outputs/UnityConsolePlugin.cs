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

            if (s_InternalLog != null)
            {
                // NoStacktrace on every log: Unity contributes no trace of its own, so what the
                // Console shows is exactly Chirp's — filtered by [HideInCallstack] and resolved
                // correctly across async/UniTask frames, which Unity's native capture is not.
                s_InternalLog(UnityLogUtil.ToUnityLogType(logEvent.Level), LogOption.NoStacktrace, FormatConsoleLog(logEvent, true), logEvent.Context);
                return;
            }

            // Fallback: the public API has no per-call LogOption, so Unity appends its own trace
            // according to the project's settings. Appending Chirp's as well would double it, so
            // Unity's is left to stand alone.
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
            }
            catch (Exception)
            {
                s_InternalLog = null;
            }
        }

        protected override void OnDispose()
        {
            if (Debug.unityLogger.logHandler == this)
                Debug.unityLogger.logHandler = m_DefaultUnityLogHandler;
            m_DefaultUnityLogHandler = null;
        }

        [ThreadStatic]
        private static StringBuilder s_HelperFormatBuilder;
        private string FormatConsoleLog(ChirpLog logEvent, bool appendStackTrace)
        {
            using var _ = s_FormatConsoleLogMarker.Auto();

            if (s_HelperFormatBuilder == null)
                s_HelperFormatBuilder = new StringBuilder();
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