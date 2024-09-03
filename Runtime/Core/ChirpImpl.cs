using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Inputs;
using WhiteSparrow.Shared.Logging.Outputs;

namespace WhiteSparrow.Shared.Logging.Core
{
    internal class ChirpImpl : IChirpReceiver, IDisposable, IChirpChannels
    {
        private List<IChirpInput> m_Inputs = new List<IChirpInput>();
        
        private List<IChirpOutput> m_Outputs = new List<IChirpOutput>();
        private IChirpOutput[] m_OutputsCached;

        public ChirpImpl()
        {
            m_DefaultChannel = CreateInternal("Default", 0);
            DefaultStyle = new ChirpStyle();
            // DefaultStyle.LogColorInfo = new Color(52f/255f, 195f/255f, 235f/255f);
            // DefaultStyle.LogColorWarning = new Color(235f/255f, 159f/255f, 52f/255f);
            // DefaultStyle.LogColorAssert = DefaultStyle.LogColorError = DefaultStyle.LogColorException = new Color(235f/255f, 88f/255f, 52f/255f);
            DefaultStyle.MarkdownInlineCodeColor = new Color(3/255f, 252/255f, 132/255f);
            DefaultStyle.MarkdownBlockCodeColor = new Color(3/255f, 252/255f, 132/255f);
        }

        #region Components

        public T AddInput<T>()
            where T : class, IChirpInput, new()
        {
            T input = new T();
            m_Inputs.Add(input);
            input.Initialize(this);

            return input;
        }
        public void AddInput(IChirpInput input)
        {
            m_Inputs.Add(input);
            input.Initialize(this);
        }

        public T AddOutput<T>()
        where T: class, IChirpOutput, new()
        {
            T output = new T();
            m_Outputs.Add(output);
            m_OutputsCached = m_Outputs.ToArray();
            output.Initialize();

            return output;
        }
        public IChirpOutput AddOutput(IChirpOutput output)
        {
            m_Outputs.Add(output);
            m_OutputsCached = m_Outputs.ToArray();
            output.Initialize();
            
            return output;
        }
        
        [HideInCallstack]
        public void Submit(ChirpLog log)
        {
            log.TimeStamp = DateTime.UtcNow;
            
            if(log.Options.AddStackTrace)
                PopulateStackTrace(log);
            
            foreach (var output in m_OutputsCached)
            {
                output.Ingest(log);
            }
            
            log.Dispose();
        }

        public void PopulateStackTrace(ChirpLog log)
        {
            StackTrace st = new StackTrace(2, true);
            if(st.FrameCount == 0)
                return;
            
            
        }

        public void Dispose()
        {
            var inputs = m_Inputs.ToArray();
            m_Inputs.Clear();

            foreach (var input in inputs)
                input.Dispose();

            var outputs = m_OutputsCached;
            m_Outputs.Clear();
            m_OutputsCached = null;
            foreach (var output in outputs)
                output.Dispose();
        }

        #endregion

        #region Channels

        private static int s_LoggerId = 1;
        private ChirpLogger m_DefaultChannel;
        ChirpLogger IChirpChannels.Default => m_DefaultChannel;

        
        public ChirpLogger Create(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            
            return CreateInternal(name, s_LoggerId++, null);
        }

        public ChirpLogger Create(string name, Color color)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));
            
            return CreateInternal(name, s_LoggerId++, color);
        }

     
        private ChirpLogger CreateInternal(string name, int id, Color? color = null)
        {
            ChirpLogger logger = color.HasValue ? new ChirpLogger(name, id, color.Value) :  new ChirpLogger(name, id);
            return logger;
        }

        #endregion

        internal ChirpStyle DefaultStyle { get; private set; }
        
    }
    
    
    public interface IChirpReceiver
    {
        void Submit(ChirpLog log);
    }

    public interface IChirpChannels
    {
        ChirpLogger Default { get; }
        ChirpLogger Create(string name);
        ChirpLogger Create(string name, Color color);
        
    }

  
}