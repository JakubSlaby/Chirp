using System;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Initialize
{
	public abstract class AbstractLoggerInitializeComponent : MonoBehaviour, IChirpLoggerInitializeComponent
	{
		public abstract void Initialize();
	}

	public interface IChirpLoggerInitializeComponent
	{
		void Initialize();
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ShowInitializeOptionsAttribute : Attribute
	{
		
	}
}