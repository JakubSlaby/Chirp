using UnityEngine;
using WhiteSparrow.Shared.Logging.Outputs;

namespace WhiteSparrow.Shared.Logging.Initialize
{
	/// <summary>
	/// Chirp Initialize Component : Unity Console Logger
	/// Used in the simple initializer only.
	/// </summary>
	[AddComponentMenu("")]
	public class ChirpUnityConsoleLogger : AbstractLoggerInitializeComponent
	{
		public override void Initialize()
		{
			Chirp.AddOutput<UnityConsolePlugin>();
		}
	}
}