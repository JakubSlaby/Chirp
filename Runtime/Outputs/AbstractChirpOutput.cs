using System;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public abstract class AbstractChirpOutput : IChirpOutput, IDisposable
    {
        public void Initialize()
        {
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
            
        }

        [HideInCallstack]
        void IChirpOutput.Ingest(ChirpLog logEvent)
        {
            if (!Filter(logEvent))
                return;
            
            Process(logEvent);
        }

        
        [HideInCallstack]
        protected abstract bool Filter(ChirpLog logEvent);
        
        [HideInCallstack]
        protected abstract void Process(ChirpLog logEvent);

        ~AbstractChirpOutput()
        {
            Dispose();
        }

        private bool m_Disposed;
        public void Dispose()
        {
            if (m_Disposed)
                return;
            OnDispose();
        }

        protected virtual void OnDispose()
        {}
    }
}