using System;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging
{
	/// <summary>
	///     Default logger for synchronising with the Unity Console.
	///     Intercepts any messages from Debug.Log calls and processes them through the Chirp framework.
	/// </summary>
	public class UnityConsoleLogger : AbstractLogger
	{
		private ILogHandler m_DefaultUnityLogHandler;

		public override void Initialize()
		{
			m_DefaultUnityLogHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = new UnityLoggerHandler();
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
			Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
		}

		public override void Destroy()
		{
			if (m_DefaultUnityLogHandler != null)
			{
				Debug.unityLogger.logHandler = m_DefaultUnityLogHandler;
				Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.Full);
				Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.Full);
				Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.Full);
				Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.Full);
			}

			m_DefaultUnityLogHandler = null;
		}
		

		~UnityConsoleLogger()
		{
			Destroy();
		}

		public override void Append(LogEvent logEvent)
		{
			if (logEvent.level == LogLevel.Exception && logEvent.exception != null)
				m_DefaultUnityLogHandler.LogException(logEvent.exception, null);
			else
				m_DefaultUnityLogHandler.LogFormat(UnityLogUtil.ToUnityLogType(logEvent.level), null,
					CreateString(logEvent));
		}

		protected override void FormatString(StringBuilder stringBuilder, LogEvent logEvent)
		{
			if (logEvent.channel != null)
			{
				stringBuilder.Append('[');
				stringBuilder.Append(logEvent.channel.name);
				stringBuilder.Append(']');
				stringBuilder.Append(' ');
			}

			base.FormatString(stringBuilder, logEvent);
			stringBuilder.AppendLine();
			stringBuilder.AppendLine(logEvent.stackTrace);
		}
	}

	/// <summary>
	///     Overwrite Log Handler for intercepting Unity Debug.Log messages.
	///     Forwards them in to Chirp assigning default channel of "Unity"
	/// </summary>
	public class UnityLoggerHandler : ILogHandler
	{
		private static readonly LogChannel s_LogChannel = new LogChannel("Unity") { isFallback = true };

		public void LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			Chirp.AddLog(s_LogChannel, UnityLogUtil.FromUnityLogType(logType), 1,string.Format(format, args));
		}

		public void LogException(Exception exception, Object context)
		{
			Chirp.AddException(s_LogChannel, LogLevel.Exception, exception);
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