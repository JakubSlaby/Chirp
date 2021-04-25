namespace WhiteSparrow.Shared.Logging
{
	public interface ILogger
	{
		void Initialize();
		void Destroy();

		void Append(LogEvent logEvent);
	}
}