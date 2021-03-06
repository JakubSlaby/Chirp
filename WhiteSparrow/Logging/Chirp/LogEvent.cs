using System;

namespace WhiteSparrow.Shared.Logging
{
	public class LogEvent
	{
		public LogChannel channel;
		public LogLevel level;
		public DateTime timeStamp;

		public object[] messages;
		public Exception exception;
	}
}