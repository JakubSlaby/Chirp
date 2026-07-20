using System;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging.Outputs
{
    public interface IChirpOutput : IChirpPlugin
    {
        /// <summary>
        /// Receives a log event for this output to process. ChirpLog instances are pooled and
        /// only valid for the duration of this call — do not retain <paramref name="logEvent"/>
        /// beyond Ingest(); use <see cref="ChirpLog.Copy"/> to keep a caller-owned snapshot.
        /// </summary>
        void Ingest(ChirpLog logEvent);
    }

}