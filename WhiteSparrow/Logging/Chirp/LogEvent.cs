using System;
using System.Diagnostics;

namespace WhiteSparrow.Shared.Logging
{
	public class LogEvent
	{
		public LogChannel channel;
		public Exception exception;
		public LogLevel level;

		public object[] messages;

		public StackTrace stackTrace;
		public DateTime timeStamp;
	}
}