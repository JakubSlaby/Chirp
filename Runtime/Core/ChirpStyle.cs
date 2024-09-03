using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Core
{
    public class ChirpStyle
    {
        // Note: All log type colors are removed from the style for now, need to figure out a nice way of handling coloured logs in Unity Editor console.
        
        private Color? m_LogColorDefault;
        private string m_LogColorDefaultHtml;
        /*
        private Color? m_LogColorDebug;
        private string m_LogColorDebugHtml;

        private Color? m_LogColorLog;
        private string m_LogColorLogHtml;

        private Color? m_LogColorInfo;
        private string m_LogColorInfoHtml;

        private Color? m_LogColorWarning;
        private string m_LogColorWarningHtml;

        private Color? m_LogColorAssert;
        private string m_LogColorAssertHtml;

        private Color? m_LogColorError;
        private string m_LogColorErrorHtml;

        private Color? m_LogColorException;
        private string m_LogColorExceptionHtml;
        */
        
        private Color? m_MarkdownInlineCodeColor;
        private string m_MarkdownInlineCodeColorHtml;
        
        private Color? m_MarkdownBlockCodeColor;
        private string m_MarkdownBlockCodeColorHtml;

        private ChirpStyle DefaultStyle
        {
            get
            {
                if (Chirp.Style == this)
                    return null;
                return Chirp.Style;
            }
        }
        public Color? LogColorDefault { 
            get => m_LogColorDefault ??  DefaultStyle?.LogColorDefault;
            set
            {
                m_LogColorDefault = value;
                m_LogColorDefaultHtml = null;
            }
        }
        /*
        public Color? LogColorDebug {
             get => m_LogColorDebug ?? m_LogColorDefault ?? DefaultStyle?.LogColorDebug;
             set
             {
                 m_LogColorDebug = value;
                 m_LogColorDebugHtml = null;
             }
         }
         public Color? LogColorLog {
             get => m_LogColorLog ?? m_LogColorDefault ?? DefaultStyle?.LogColorLog;
             set
             {
                 m_LogColorLog = value;
                 m_LogColorLogHtml = null;
             }
         }
         public Color? LogColorInfo {
             get => m_LogColorInfo ?? m_LogColorDefault ?? DefaultStyle?.LogColorInfo;
             set
             {
                 m_LogColorInfo = value;
                 m_LogColorInfoHtml = null;
             }
         }
         public Color? LogColorWarning {
             get => m_LogColorWarning ?? DefaultStyle?.LogColorWarning ?? LogColorDefault;
             set
             {
                 m_LogColorWarning = value;
                 m_LogColorWarningHtml = null;
             }
         }
         public Color? LogColorAssert {
             get => m_LogColorAssert ?? DefaultStyle?.LogColorAssert ?? LogColorDefault;
             set
             {
                 m_LogColorAssert = value;
                 m_LogColorAssertHtml = null;
             }
         }
         public Color? LogColorError {
             get => m_LogColorError ?? DefaultStyle?.LogColorError ?? LogColorDefault;
             set
             {
                 m_LogColorError = value;
                 m_LogColorErrorHtml = null;
             }
         }
         public Color? LogColorException {
             get => m_LogColorException ?? DefaultStyle?.LogColorException ?? LogColorDefault;
             set
             {
                 m_LogColorException = value;
                 m_LogColorExceptionHtml = null;
             }
         }
         */
        
        public Color? MarkdownInlineCodeColor { 
            get => m_MarkdownInlineCodeColor ?? DefaultStyle?.MarkdownInlineCodeColor ?? LogColorDefault;
            set
            {
                m_MarkdownInlineCodeColor = value;
                m_MarkdownInlineCodeColorHtml = null;
            }
        }
        
        public Color? MarkdownBlockCodeColor { 
            get => m_MarkdownBlockCodeColor ?? DefaultStyle?.MarkdownBlockCodeColor ?? LogColorDefault;
            set
            {
                m_MarkdownBlockCodeColor = value;
                m_MarkdownBlockCodeColorHtml = null;
            }
        }
        
        
        public string LogColorDefaultHtml => m_LogColorDefaultHtml ??= GetCachedHtmlColor(LogColorDefault);
        
        /*
        public string LogColorDebugHtml => m_LogColorDebugHtml ??= GetCachedHtmlColor(LogColorDebug);
        public string LogColorLogHtml => m_LogColorLogHtml ??= GetCachedHtmlColor(LogColorLog);
        public string LogColorInfoHtml => m_LogColorInfoHtml = GetCachedHtmlColor(LogColorInfo);
        public string LogColorWarningHtml => m_LogColorWarningHtml ??= GetCachedHtmlColor(LogColorWarning);
        public string LogColorAssertHtml => m_LogColorAssertHtml ??= GetCachedHtmlColor(LogColorAssert);
        public string LogColorErrorHtml => m_LogColorErrorHtml ??= GetCachedHtmlColor(LogColorError);
        public string LogColorExceptionHtml => m_LogColorExceptionHtml ??= GetCachedHtmlColor(LogColorException);
        */
        public string MarkdownInlineCodeColorHtml => m_MarkdownInlineCodeColorHtml ??= GetCachedHtmlColor(MarkdownInlineCodeColor);
        public string MarkdownBlockCodeColorHtml => m_MarkdownBlockCodeColorHtml ??= GetCachedHtmlColor(MarkdownBlockCodeColor);
        
        public bool TryGetColor(LogLevel level, out Color color)
        {
            color = default;
            return false;
            /*
            Color? output = null;

            switch (level)
            {
                case LogLevel.Debug:
                    output = LogColorDebug;
                    break;
                case LogLevel.Log:
                    output = LogColorLog;
                    break;
                case LogLevel.Info:
                    output = LogColorInfo;
                    break;
                case LogLevel.Warning:
                    output = LogColorWarning;
                    break;
                case LogLevel.Assert:
                    output = LogColorAssert;
                    break;
                case LogLevel.Error:
                    output = LogColorError;
                    break;
                case LogLevel.Exception:
                    output = LogColorException;
                    break;
            }

            if (!output.HasValue)
            {
                color = default;
                return false;
            }

            color = output.Value;
            return true;*/
        }
        public bool TryGetColorHtml(LogLevel level, out string color)
        {
            color = null;
            return false;
            /*
            string output = null;
            switch (level)
            {
                case LogLevel.Debug:
                    output = LogColorDebugHtml;
                    break;
                case LogLevel.Log:
                    output = LogColorLogHtml;
                    break;
                case LogLevel.Info:
                    output = LogColorInfoHtml;
                    break;
                case LogLevel.Warning:
                    output = LogColorWarningHtml;
                    break;
                case LogLevel.Assert:
                    output = LogColorAssertHtml;
                    break;
                case LogLevel.Error:
                    output = LogColorErrorHtml;
                    break;
                case LogLevel.Exception:
                    output = LogColorExceptionHtml;
                    break;
            }

            color = output;
            return color != null;
            */
        }
        

        public string GetCachedHtmlColor(Color? input)
        {
            if (!input.HasValue)
                return null;
            return ColorUtility.ToHtmlStringRGB(input.Value);
        }
        
    }

    public class ChirpLogStyle
    {
        public Color? LogColor;
    }
    
}