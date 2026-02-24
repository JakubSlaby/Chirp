using System;

namespace WhiteSparrow.Shared.Logging.Core
{
	public interface IChirpPlugin : IDisposable
	{
		event Action<IChirpPlugin> OnDisposed;
	}
}