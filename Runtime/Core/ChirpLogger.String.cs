using System;
using System.Diagnostics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging.Core
{
    public partial class ChirpLogger
    {
        
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Debug(string message, Object context = null) => Debug(ChirpLogUtil.ConstructLog(message, context));

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Log(string message, Object context = null) => Log(ChirpLogUtil.ConstructLog(message, context));

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Info(string message, Object context = null) => Info(ChirpLogUtil.ConstructLog(message, context));

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Warning(string message, Object context = null) => Warning(ChirpLogUtil.ConstructLog(message, context));
		
#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
		[Conditional("LogLevel4"), Conditional("LogLevelAssert")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Assert(bool condition, string message, Object context = null)
		{
			if (condition)
				return;
			Assert(ChirpLogUtil.ConstructLog(message,context));
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
	    [HideInCallstack]
	    public void Error(string message, Object context = null) => Error(ChirpLogUtil.ConstructLog(message, context));
		
		
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
		[HideInCallstack]
		public void Exception(Exception exception, Object context = null)
		{
			var log = ChirpLogUtil.ConstructLog(exception.Message, context);
			log.Source = this;
			log.StackTrace = exception.StackTrace;
			log.Level = LogLevel.Exception;
			Chirp.Impl.Submit(log);
		}
    }
}