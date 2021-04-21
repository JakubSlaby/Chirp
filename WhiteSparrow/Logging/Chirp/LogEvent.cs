using System;

namespace WhiteSparrow.Shared.Logging
{
	public class LogEvent
	{
		public LogChannel channel;
		public Exception exception;
		public LogLevel level;

		public object[] messages;

		public string stackTrace;
		public DateTime timeStamp;
	}
}