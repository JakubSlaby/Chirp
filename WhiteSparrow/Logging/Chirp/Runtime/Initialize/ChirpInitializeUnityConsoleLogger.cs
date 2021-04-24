using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Initialize
{
	/// <summary>
	/// Chirp Initialize Component : Unity Console Logger
	/// Used in the simple initializer only.
	/// </summary>
	[AddComponentMenu("")]
	public class ChirpInitializeUnityConsoleLogger : AbstractLoggerInitializeComponent<UnityConsoleLogger>
	{
		public override UnityConsoleLogger GetInstance()
		{
			return new UnityConsoleLogger();
		}
	}
}