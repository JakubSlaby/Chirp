using System;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public interface IChirpOutput : IDisposable
    {
        void Initialize();
        void Ingest(ChirpLog logEvent);
    }

}