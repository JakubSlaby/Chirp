using System;

namespace WhiteSparrow.Shared.Logging.Core
{
	public class AbstractChirpPlugin : IChirpPlugin
	{
		private bool m_Disposed = false;
		private Action<IChirpPlugin> m_OnDisposed;
		public event Action<IChirpPlugin> OnDisposed
		{
			add
			{
				if(m_Disposed)
					value.Invoke(this);
				else
					m_OnDisposed += value;
			}
			remove => m_OnDisposed -= value;
		}
		
		public void Dispose()
		{
			if (m_Disposed)
				return;

			m_Disposed = true;
			m_OnDisposed?.Invoke(this);
			m_OnDisposed = null;
			OnDispose();
		}

		protected virtual void OnDispose()
		{
			
		}
	}
}