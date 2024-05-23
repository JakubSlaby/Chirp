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
	public class UnityLogger : AbstractLogger, ILogHandler
	{
		private ILogHandler m_DefaultUnityLogHandler;

		public override void Initialize()
		{
			m_DefaultUnityLogHandler = Debug.unityLogger.logHandler;
			Debug.unityLogger.logHandler = this;
		}

		public override void Destroy()
		{
			if (m_DefaultUnityLogHandler != null)
			{
				Debug.unityLogger.logHandler = m_DefaultUnityLogHandler;
			}

			m_DefaultUnityLogHandler = null;
		}
		

		~UnityLogger()
		{
			Destroy();
		}

		[HideInCallstack]
		public override void Append(LogEvent logEvent)
		{
			if (logEvent.level == LogLevel.Exception && logEvent.exception != null)
				m_DefaultUnityLogHandler.LogException(logEvent.exception, null);
			else
				m_DefaultUnityLogHandler.LogFormat(UnityLogUtil.ToUnityLogType(logEvent.level), null,  "{0}",CreateString(logEvent));
		}

		protected override void FormatString(StringBuilder stringBuilder, LogEvent logEvent)
		{
			if (logEvent.channel != null)
			{
				stringBuilder.Append($"[<color=#{ColorUtility.ToHtmlStringRGB(logEvent.channel.color)}>");
				stringBuilder.Append(logEvent.channel.name);
				stringBuilder.Append("</color>] ");
			}

			base.FormatString(stringBuilder, logEvent);
		}

		private static readonly LogChannel s_LogChannel = new LogChannel("Unity") { isFallback = true };
		[HideInCallstack]
		void ILogHandler.LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			Chirp.AddLog(s_LogChannel, UnityLogUtil.FromUnityLogType(logType), 1, string.Format(format, args));
		}

		[HideInCallstack]
		void ILogHandler.LogException(Exception exception, Object context)
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