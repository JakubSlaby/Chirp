namespace WhiteSparrow.Shared.Logging
{
	public interface ILogger
	{
		void Initialise();
		void Destroy();

		void Append(LogEvent logEvent);
	}
}