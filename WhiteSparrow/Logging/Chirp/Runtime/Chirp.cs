using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
#if UNITY_EDITOR
using UnityEditor;
#endif



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
		public const string Version = "0.8.4";

		private static ILogger[] s_Loggers;
		
		[Conditional("CHIRP")]
		public static void Initialize(params ILogger[] loggers)
		{
			if (loggers == null || loggers.Length == 0)
				return;
			Initialize((IEnumerable<ILogger>)loggers);
		}
		
		[Conditional("CHIRP")]
		public static void Initialize(IEnumerable<ILogger> loggers)
		{
			List<ILogger> loggersWrapper = new List<ILogger>(loggers);
			s_Loggers = loggersWrapper.ToArray();

			foreach (var logger in s_Loggers)
				logger.Initialise();

			Debug($"Chirp v{Version} Initialised.\nIncluded Loggers: {ToStringLoggers()}");

#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
			EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
		}

		private static string ToStringLoggers()
		{
			var outputListOfLoggers = new StringBuilder();
			foreach (var logger in s_Loggers) outputListOfLoggers.AppendLine(logger.GetType().Name);

			return outputListOfLoggers.ToString();
		}
#if UNITY_EDITOR
		private static void OnPlayModeChanged(PlayModeStateChange obj)
		{
			if (obj == PlayModeStateChange.ExitingPlayMode)
				Destroy();
		}
#endif

		public static void Destroy()
		{
#if UNITY_EDITOR
			EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif

			foreach (var logger in s_Loggers) logger.Destroy();

			s_Loggers = Array.Empty<ILogger>();
		}
		
		
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Debug(params object[] message)
		{
			AddLog(null, LogLevel.Debug, message);
		}


#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
#else
		[Conditional("CHIRP")]
#endif
		public static void DebugCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Debug, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Log(params object[] message)
		{
			AddLog(null, LogLevel.Log, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
#else
		[Conditional("CHIRP")]
#endif
		public static void LogCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Log, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Info(params object[] message)
		{
			AddLog(null, LogLevel.Info, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
#else
		[Conditional("CHIRP")]
#endif
		public static void InfoCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Info, message);
		}
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Warning(params object[] message)
		{
			AddLog(null, LogLevel.Warning, message);
		}
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
#else
		[Conditional("CHIRP")]
#endif
		public static void WarningCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Warning, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Assert(bool condition)
		{
			if (condition)
				return;
			AddLog(null, LogLevel.Assert, "Assertion Failed");
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Assert(bool condition, params object[] message)
		{
			if (condition)
				return;
			AddLog(null, LogLevel.Assert, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Assert(LogChannel channel, bool condition)
		{
			if (condition)
				return;
			AddLog(channel, LogLevel.Assert, "Assertion Failed");
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Assert(LogChannel channel, bool condition, params object[] message)
		{
			if (condition)
				return;
			AddLog(channel, LogLevel.Assert, message);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		[Conditional("LogLevel5"), Conditional("LogLevelError")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Error(params object[] message)
		{
			AddLog(null, LogLevel.Error, message);
		}
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		[Conditional("LogLevel5"), Conditional("LogLevelError")]
#else
		[Conditional("CHIRP")]
#endif
		public static void ErrorCh(LogChannel channel, params object[] message)
		{
			AddLog(channel, LogLevel.Error, message);
		}
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		[Conditional("LogLevel5"), Conditional("LogLevelError")]
		[Conditional("LogLevel6"), Conditional("LogLevelException")]
		[Conditional("CHIRP")]
#else
		[Conditional("CHIRP")]
#endif
		public static void Exception(Exception exception, params object[] message)
		{
			AddException(null, LogLevel.Exception, exception, message);
		}
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
		[Conditional("LogLevel5"), Conditional("LogLevelError")]
		[Conditional("LogLevel6"), Conditional("LogLevelException")]
		[Conditional("CHIRP")]
#else
		[Conditional("CHIRP")]
#endif
		public static void ExceptionCh(LogChannel channel, Exception exception, params object[] message)
		{
			AddException(channel, LogLevel.Exception, exception, message);
		}

		internal static void AddLog(LogChannel channel, LogLevel logLevel, params object[] message)
		{
			AddLog(channel, logLevel, 0, message);
		}
		
		internal static void AddLog(LogChannel channel, LogLevel logLevel, int skipFrames, params object[] message)
		{
			if (!AttemptLogAppend())
				return;

			var logEvent = ConstructLogEvent(channel, logLevel, null, skipFrames, message);

			for (int i = 0, l = s_Loggers.Length; i < l; i++)
				s_Loggers[i].Append(logEvent);
		}

		internal static void AddException(LogChannel channel, LogLevel logLevel, Exception exception,
			params object[] messages)
		{
			if (!AttemptLogAppend())
				return;

			var logEvent = ConstructLogEvent(channel, logLevel, exception, 0, messages);

			for (int i = 0, l = s_Loggers.Length; i < l; i++)
				s_Loggers[i].Append(logEvent);
		}

		private static LogEvent ConstructLogEvent(LogChannel channel, LogLevel logLevel, Exception exception,
			int skipFrames, params object[] messages)
		{
			var evt = new LogEvent();

			evt.channel = channel;
			evt.level = logLevel;
			evt.timeStamp = DateTime.UtcNow;
			evt.messages = messages;
			evt.exception = exception;

			StackTrace stackTrace = exception != null ? new StackTrace(exception) : new StackTrace(3 + skipFrames, true);
			evt.stackTrace = LoggingStackTraceUtil.FormatUnityStackTrace(stackTrace, out LogChannel foundChannel);
			if ((evt.channel == null || evt.channel.isFallback) && foundChannel != null)
				evt.channel = foundChannel;
			
			return evt;
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
	}
}