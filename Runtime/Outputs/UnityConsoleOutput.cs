using System;
using System.IO;
using System.Text;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;
using WhiteSparrow.Shared.Logging.Inputs;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public class UnityConsoleOutput : AbstractChirpOutput, IChirpInput, ILogHandler
    {
        private ILogHandler m_DefaultUnityLogHandler;
        private IChirpReceiver m_Receiver;
        private ChirpLogger m_Channel;
        
        public UnityConsoleOutput()
        {
            m_DefaultUnityLogHandler = Debug.unityLogger.logHandler;
        }
        
        void IChirpInput.Initialize(IChirpReceiver receiver)
        {
            m_Receiver = receiver;
            m_Channel = Chirp.Channels.Create("Unity");
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
            m_DefaultUnityLogHandler = null;
        }

        [ThreadStatic]
        private static StringBuilder s_HelperFormatBuilder;
        private string FormatConsoleLog(ChirpLog logEvent)
        {
            if (s_HelperFormatBuilder == null)
                s_HelperFormatBuilder = new StringBuilder();
            else
                s_HelperFormatBuilder.Clear();

            if (logEvent.Source.LoggerId != 0)
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

            return s_HelperFormatBuilder.ToString();
        }


        [HideInCallstack]
        void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            var log = ChirpLogUtil.ConstructLog(string.Format(format, args), context);
            log.Level = UnityLogUtil.FromUnityLogType(logType);
            log.Source = m_Channel;
            m_Receiver.Submit(log);
            
            
        }

        [HideInCallstack]
        void ILogHandler.LogException(Exception exception, Object context)
        {
            var log = ChirpLogUtil.ConstructLog(exception.Message, context);
            log.Source = m_Channel;
            log.StackTrace = exception.StackTrace;
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