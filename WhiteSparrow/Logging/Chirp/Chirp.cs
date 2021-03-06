using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WhiteSparrow.Shared.Logging
{
	public enum LogLevel
	{
		Debug = 0,
		Log = 1,
		Warning = 2,
		Assert = 3,
		Error = 4,
		Exception = 5
	}
	
	public static class Chirp
	{
		
		private static ILogger[] s_Loggers;
		
	
		public static void Initialise(params ILogger[] loggers)
		{
			if (loggers == null)
				return;

			Logging.Chirp.s_Loggers = loggers;
			
			List<ILogger> logs = new List<ILogger>();
			
			
			foreach (var logger in s_Loggers)
				logger.Initialise();
		}

		public static void Destroy()
		{
			foreach (var logger in s_Loggers)
			{
				logger.Destroy();
			}

			s_Loggers = Array.Empty<ILogger>();
		}

		[Conditional("DEBUG")]
		public static void Debug(params object[] message)
		{
			AddLog(null, LogLevel.Debug, message);
		}
		[Conditional("DEBUG")]
		public static void DebugCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Debug, message);
		}

		public static void Log(params object[] message)
		{
			AddLog(null, LogLevel.Log, message);
		}

		public static void LogCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Log, message);
		}

		public static void Warning(params object[] message)
		{
			AddLog(null, LogLevel.Warning, message);
		}
		public static void WarningCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Warning, message);
		}
		
		public static void Error(params object[] message)
		{
			AddLog(null, LogLevel.Error, message);
		}
		public static void ErrorCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Error, message);
		}

		public static void Exception(Exception exception, params object[] message)
		{
			AddException(null, LogLevel.Exception, exception, message);
		}
		public static void ExceptionCh(LogChannel channel, Exception exception, params object[] message)
		{
			AddException(channel, LogLevel.Exception, exception, message);
		}

		internal static void AddLog(LogChannel channel, LogLevel logLevel, params object[] message)
		{
			if (s_Loggers == null || s_Loggers.Length == 0)
				return;

			LogEvent logEvent = ConstructLogEvent(channel, logLevel, null, message);
			
			for (int i = 0, l = s_Loggers.Length; i < l; i++)
				s_Loggers[i].Append(logEvent);
		}

		internal static void AddException(LogChannel channel, LogLevel logLevel, Exception exception, params object[] messages)
		{
			if (s_Loggers == null || s_Loggers.Length == 0)
				return;

			LogEvent logEvent = ConstructLogEvent(channel, logLevel, exception, messages);
			
			for (int i = 0, l = s_Loggers.Length; i < l; i++)
				s_Loggers[i].Append(logEvent);

		}

		private static LogEvent ConstructLogEvent(LogChannel context, LogLevel logLevel, Exception exception, params object[] messages)
		{
			LogEvent evt = new LogEvent();

			evt.channel = context;
			evt.level = logLevel;
			evt.timeStamp = DateTime.UtcNow;
			evt.messages = messages;
			evt.exception = exception;
			
			return evt;
		}
	}
}