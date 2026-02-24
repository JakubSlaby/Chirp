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
		public void Debug(ChirpLog log)
		{
			log.Source = this;
			log.Level = LogLevel.Debug;
			Chirp.Impl.Submit(log);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Log(ChirpLog log) 
		{
			log.Source = this;
			log.Level = LogLevel.Log;
			Chirp.Impl.Submit(log);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Info(ChirpLog log)
		{
			log.Source = this;
			log.Level = LogLevel.Info;
			Chirp.Impl.Submit(log);
		}

#if CHIRP
		[Conditional("LogLevel0"), Conditional("LogLevelDebug")]
		[Conditional("LogLevel1"), Conditional("LogLevelDefault")]
		[Conditional("LogLevel2"), Conditional("LogLevelInfo")]
		[Conditional("LogLevel3"), Conditional("LogLevelWarning")]
#else
		[Conditional("CHIRP")]
#endif
		[HideInCallstack]
		public void Warning(ChirpLog log)
		{
			log.Source = this;
			log.Level = LogLevel.Warning;
			Chirp.Impl.Submit(log);
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
		[HideInCallstack]
		public void Assert(bool condition, ChirpLog log)
		{
			if (condition)
				return;
			Assert(log);
		}
		private void Assert(ChirpLog log)
		{
			log.Source = this;
			log.Level = LogLevel.Assert;
			Chirp.Impl.Submit(log);
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
		public void Error(ChirpLog log)
		{
			log.Source = this;
			log.Level = LogLevel.Error;
			Chirp.Impl.Submit(log);
		}

	}

}