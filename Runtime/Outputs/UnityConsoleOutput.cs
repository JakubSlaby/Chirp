using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public class UnityConsoleOutput : AbstractChirpOutput
    {
        private ILogHandler m_DefaultUnityLogHandler;
        public UnityConsoleOutput()
        {
            m_DefaultUnityLogHandler = Debug.unityLogger.logHandler;
        }
        
        protected override bool Filter(ChirpLog logEvent)
        {
            return true;
        }

        [HideInCallstack]
        protected override void Process(ChirpLog logEvent)
        {
            UnityEngine.Debug.LogFormat(UnityLogUtil.ToUnityLogType(logEvent.Level), LogOption.NoStacktrace,null, "{0}", FormatConsoleLog(logEvent));
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