using System;
using System.Text;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;
using WhiteSparrow.Shared.Logging.Inputs;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public class UnityConsolePlugin : AbstractChirpOutput, IChirpInput, ILogHandler
    {
        private static readonly LogType[] s_AllLogTypes = (LogType[])Enum.GetValues(typeof(LogType));

        private static ILogHandler m_DefaultUnityLogHandler;
        
        
        private IChirpReceiver m_Receiver;
        private ChirpLogger m_Channel;
        private StackTraceLogType[] m_PreviousStackTraceLogTypes;

        public UnityConsolePlugin()
        {
            if(m_DefaultUnityLogHandler == null)
                m_DefaultUnityLogHandler = Debug.unityLogger.logHandler;
        }

        void IChirpInput.InitializeInput(IChirpReceiver receiver)
        {
            m_Receiver = receiver;
            m_Channel = new ChirpLogger("Unity");

            m_PreviousStackTraceLogTypes = new StackTraceLogType[s_AllLogTypes.Length];
            for (int i = 0; i < s_AllLogTypes.Length; i++)
            {
                var logType = s_AllLogTypes[i];
                m_PreviousStackTraceLogTypes[i] = Application.GetStackTraceLogType(logType);
                Application.SetStackTraceLogType(logType, StackTraceLogType.None);
            }

            Debug.unityLogger.logHandler = this;
        }
        
        protected override void OnInitialize()
        {
        }

        protected override bool Filter(ChirpLog logEvent)
        {
            return true;
        }

        [HideInCallstack]
        protected override void Process(ChirpLog logEvent)
        {
            m_DefaultUnityLogHandler.LogFormat(UnityLogUtil.ToUnityLogType(logEvent.Level), logEvent.Context, "{0}", FormatConsoleLog(logEvent));
        }

        protected override void OnDispose()
        {
            if (Debug.unityLogger.logHandler == this)
                Debug.unityLogger.logHandler = m_DefaultUnityLogHandler;
            m_DefaultUnityLogHandler = null;

            if (m_PreviousStackTraceLogTypes != null)
            {
                for (int i = 0; i < s_AllLogTypes.Length; i++)
                    Application.SetStackTraceLogType(s_AllLogTypes[i], m_PreviousStackTraceLogTypes[i]);
                m_PreviousStackTraceLogTypes = null;
            }
        }

        [ThreadStatic]
        private static StringBuilder s_HelperFormatBuilder;
        private string FormatConsoleLog(ChirpLog logEvent)
        {
            if (s_HelperFormatBuilder == null)
                s_HelperFormatBuilder = new StringBuilder();
            else
                s_HelperFormatBuilder.Clear();

            if (logEvent.Source is { UseChannel: true })
            {
                s_HelperFormatBuilder.Append('[');
                s_HelperFormatBuilder.AppendFormat("<color=#{0}>", logEvent.Source.ColorHtml);
                s_HelperFormatBuilder.Append(logEvent.Source.Name);
                s_HelperFormatBuilder.Append("</color>");
                s_HelperFormatBuilder.Append(']');
                s_HelperFormatBuilder.Append(' ');
            }

            if (logEvent.Options.HasMarkdown)
            {
                s_HelperFormatBuilder.Append(LoggingMarkdownUtil.Parse(logEvent.Message, logEvent.Source.Style));
            }
            else
            {
                s_HelperFormatBuilder.Append(logEvent.Message);
            }
            
            if (!string.IsNullOrWhiteSpace(logEvent.StackTrace))
            {
                s_HelperFormatBuilder.AppendLine();
                s_HelperFormatBuilder.Append(logEvent.StackTrace);
            }

            return s_HelperFormatBuilder.ToString();
        }


        [HideInCallstack]
        void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var log = ChirpLogUtil.ConstructLog(string.Format(format, args), context);
            log.Level = UnityLogUtil.FromUnityLogType(logType);
            log.Source = m_Channel;
            log.StackTrace = LoggingStackTraceUtil.FormatUnityStackTrace(new System.Diagnostics.StackTrace(true));
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