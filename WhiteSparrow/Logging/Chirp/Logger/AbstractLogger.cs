using System;
using System.Text;

namespace WhiteSparrow.Shared.Logging
{
	public abstract class AbstractLogger : ILogger
	{
		[ThreadStatic]
		private readonly StringBuilder m_StringBuilder = new StringBuilder();

		public abstract void Initialise();

		public abstract void Destroy();

		public abstract void Append(LogEvent logEvent);

		protected string CreateString(LogEvent logEvent)
		{
			var stringBuilder = m_StringBuilder.Clear();
			FormatString(stringBuilder, logEvent);
			return stringBuilder.ToString();
		}

		protected virtual void FormatString(StringBuilder stringBuilder, LogEvent logEvent)
		{
			foreach (var message in logEvent.messages) 
				stringBuilder.Append(message);
		}

	}
}