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
        private List<IChirpPlugin> m_Plugins = new List<IChirpPlugin>();
        private List<IChirpInput> m_Inputs = new List<IChirpInput>();
        private List<IChirpOutput> m_Outputs = new List<IChirpOutput>();

        public ChirpImpl()
        {
            m_DefaultChannel = new ChirpLogger("Default", useChannel: false);
            DefaultStyle = new ChirpStyle();
            // DefaultStyle.LogColorInfo = new Color(52f/255f, 195f/255f, 235f/255f);
            // DefaultStyle.LogColorWarning = new Color(235f/255f, 159f/255f, 52f/255f);
            // DefaultStyle.LogColorAssert = DefaultStyle.LogColorError = DefaultStyle.LogColorException = new Color(235f/255f, 88f/255f, 52f/255f);
            DefaultStyle.MarkdownInlineCodeColor = new Color(3/255f, 252/255f, 132/255f);
            DefaultStyle.MarkdownBlockCodeColor = new Color(3/255f, 252/255f, 132/255f);
        }

        #region Components

        public T AddPlugin<T>()
            where T : class, IChirpPlugin, new()
        {
            T instance =  new T();
            AddPlugin(instance);
            return instance;
            
        }

        public void AddPlugin(IChirpPlugin instance)
        {
            if (instance is IChirpInput input)
            {
                m_Inputs.Add(input);
                input.InitializeInput(this);
            }

            if (instance is IChirpOutput output)
            {
                m_Outputs.Add(output);
            }
            
            RegisterPlugin(instance);
        }

        private void RegisterPlugin(IChirpPlugin instance)
        {
            if (m_Plugins.Contains(instance))
                return;
            
            m_Plugins.Add(instance);
            instance.OnDisposed += OnPluginDisposed;
        }

        private void OnPluginDisposed(IChirpPlugin instance)
        {
            RemovePlugin(instance);
        }

        public void RemovePlugin(IChirpPlugin instance)
        {
            if (!m_Plugins.Contains(instance))
                return;

            instance.OnDisposed -= OnPluginDisposed;
            m_Plugins.Remove(instance);

            if (instance is IChirpInput input)
                m_Inputs.Remove(input);
            if (instance is IChirpOutput output)
                m_Outputs.Remove(output);
        }

        [HideInCallstack]
        public void Submit(ChirpLog log)
        {
            log.TimeStamp = DateTime.UtcNow;
            
            if(log.Options.AddStackTrace || log.Level >= LogLevel.Assert)
                PopulateStackTrace(log);

            foreach (var output in m_Outputs)
            {
                output.Ingest(log);
            }
            
            log.Dispose();
        }

        [HideInCallstack]
        public void PopulateStackTrace(ChirpLog log)
        {
            if (log.StackTrace != null)
                return;

            log.StackTrace = LoggingStackTraceUtil.ExtractStackTrace();
        }

        public void Dispose()
        {
            var plugins = m_Plugins.ToArray();
            m_Plugins.Clear();
            m_Inputs.Clear();
            m_Outputs.Clear();

            foreach (var plugin in plugins)
            {
                plugin.Dispose();
            }
        }

        #endregion

        #region Channels

        private ChirpLogger m_DefaultChannel;
        ChirpLogger IChirpChannels.Default => m_DefaultChannel;

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
        
    }

  
}