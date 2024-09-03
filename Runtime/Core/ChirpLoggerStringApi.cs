using System;
using System.Diagnostics;
using UnityEngine;

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
		public void Debug(string message) => Debug(ChirpLogUtil.ConstructLog(message));

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Log(string message) => Log(ChirpLogUtil.ConstructLog(message));

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Info(string message) => Info(ChirpLogUtil.ConstructLog(message));

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Warning(string message) => Warning(ChirpLogUtil.ConstructLog(message));
		
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
		public void Assert(bool condition, string message)
		{
			if (condition)
				return;
			Assert(ChirpLogUtil.ConstructLog(message));
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
	    public void Error(string message) => Error(ChirpLogUtil.ConstructLog(message));
		
		
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
		public void Exception(Exception exception)
		{
			var log = ChirpLogUtil.ConstructLog(exception.Message);
			log.Source = this;
			log.StackTrace = exception.StackTrace;
			log.Level = LogLevel.Exception;
			Chirp.Impl.Submit(log);
		}
    }
}