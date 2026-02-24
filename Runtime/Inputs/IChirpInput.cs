using System;
using WhiteSparrow.Shared.Logging.Core;

namespace WhiteSparrow.Shared.Logging.Inputs
{
    public interface IChirpInput : IChirpPlugin
    {
        void InitializeInput(IChirpReceiver receiver);
    }
}