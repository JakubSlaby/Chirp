using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace WhiteSparrow.Shared.Logging
{
	public enum LogLevel
	{
		Debug = 0,
		Log = 1,
		Info = 2,
		Warning = 3,
		Assert = 4,
		Error = 5,
		Exception = 6
	}
	
	public static class Chirp
	{
		private static ILogger[] s_Loggers;
		
		public static void Initialize(params ILogger[] loggers)
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
		

		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		public static void Debug(params object[] message)
		{
			AddLog(null, LogLevel.Debug, message);
		}
		
		
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		public static void DebugCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Debug, message);
		}
		
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		public static void Log(params object[] message)
		{
			AddLog(null, LogLevel.Log, message);
		}

		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		public static void LogCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Log, message);
		}

		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		public static void Info(params object[] message)
		{
			AddLog(null, LogLevel.Info, message);
		}

		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		public static void InfoCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Info, message);
		}

		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		public static void Warning(params object[] message)
		{
			AddLog(null, LogLevel.Warning, message);
		}
		public static void WarningCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Warning, message);
		}

		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		public static void Assert(bool condition)
		{
			if (condition)
				return;
			AddLog(null, LogLevel.Assert, "Assertion Failed");
		}
		
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		public static void Assert(bool condition, params object[] message)
		{
			if (condition)
				return;
			AddLog(null, LogLevel.Assert, message);
		}
		
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		public static void Assert(LogChannel channel, bool condition)
		{
			if (condition)
				return;
			AddLog(channel, LogLevel.Assert, "Assertion Failed");
		}
		
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		public static void Assert(LogChannel channel, bool condition, params object[] message)
		{
			if (condition)
				return;
			AddLog(channel, LogLevel.Assert, message);
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
			if (!AttemptLogAppend())
				return;
			

			LogEvent logEvent = ConstructLogEvent(channel, logLevel, null, message);
			
			for (int i = 0, l = s_Loggers.Length; i < l; i++)
				s_Loggers[i].Append(logEvent);
		}

		internal static void AddException(LogChannel channel, LogLevel logLevel, Exception exception, params object[] messages)
		{
			if (!AttemptLogAppend())
				return;

			LogEvent logEvent = ConstructLogEvent(channel, logLevel, exception, messages);
			
			for (int i = 0, l = s_Loggers.Length; i < l; i++)
				s_Loggers[i].Append(logEvent);

		}

		private static bool AttemptLogAppend()
		{
			if (s_Loggers == null || s_Loggers.Length == 0)
			{
				#if UNITY_EDITOR
				UnityEngine.Debug.LogError("Attempting to use Chirp logger with no Loggers. Call Chirp.Initialize() before using.");
				#endif
				
				return false;
			}
			
			return true;
		}

		private static LogEvent ConstructLogEvent(LogChannel channel, LogLevel logLevel, Exception exception, params object[] messages)
		{
			LogEvent evt = new LogEvent();

			evt.channel = channel;
			evt.level = logLevel;
			evt.timeStamp = DateTime.UtcNow;
			evt.messages = messages;
			evt.exception = exception;
			evt.stackTrace = exception != null ? new StackTrace(exception) : new StackTrace(3, true);

			if (channel == null || channel.isFallback)
			{
				for (int i = 0; i < evt.stackTrace.FrameCount; i++)
				{
					var frame = evt.stackTrace.GetFrame(i);
					var method = frame?.GetMethod();
					if (method == null)
						continue;
					var methodType = method.DeclaringType;
					var c = LogChannel.GetForTarget(methodType);
					if (c != null)
					{
						evt.channel = c;
						break;
					}
				}
			}
			
			return evt;
		}
	}
}