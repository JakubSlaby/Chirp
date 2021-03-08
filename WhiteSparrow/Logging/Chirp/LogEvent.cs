using System;
using System.Diagnostics;

namespace WhiteSparrow.Shared.Logging
{
	public class LogEvent
	{
		public LogChannel channel;
		public LogLevel level;
		public DateTime timeStamp;

		public object[] messages;
		public Exception exception;

		public StackTrace stackTrace;
	}
}