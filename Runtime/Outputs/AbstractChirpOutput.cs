using System;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public abstract class AbstractChirpOutput : AbstractChirpPlugin, IChirpOutput
    {
        public void InitializeOutput()
        {
            OnInitialize();
        }

        protected abstract void OnInitialize();

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

    }
}