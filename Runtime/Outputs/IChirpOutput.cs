using System;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public interface IChirpOutput : IChirpPlugin
    {
        void Ingest(ChirpLog logEvent);
    }

}