using System;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Initialize
{
	public abstract class AbstractLoggerInitializeComponent<T> : MonoBehaviour, IChirpLoggerInitializeComponent
		where T : ILogger
	{
		public ILogger GetLoggerInstance()
		{
			return GetInstance();
		}
		
		public abstract T GetInstance();
	}

	public interface IChirpLoggerInitializeComponent
	{
		ILogger GetLoggerInstance();
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class ShowInitializeOptionsAttribute : Attribute
	{
		
	}
}