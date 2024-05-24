using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Initialize
{
	/// <summary>
	/// Chirp Initialize Component : Unity Console Logger
	/// Used in the simple initializer only.
	/// </summary>
	[AddComponentMenu("")]
	public class ChirpInitializeUnityConsoleLogger : AbstractLoggerInitializeComponent<UnityLogger>
	{
		public override UnityLogger GetInstance()
		{
			return new UnityLogger();
		}
	}
}