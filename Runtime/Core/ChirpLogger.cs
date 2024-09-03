﻿using System;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Core
{
    public partial class ChirpLogger : IDisposable
    {
	    public readonly int LoggerId;
	    public readonly string Name;
	    public readonly Color ChannelColor;
	    public readonly string ColorHtml;

	    public readonly ChirpStyle Style;
	    
	    internal ChirpLogger(string name, int id)
	    {
		    Name = name;
		    LoggerId = id;
		    ChannelColor = CreateColorHash(name);
		    ColorHtml = ColorUtility.ToHtmlStringRGB(ChannelColor);
		    Style = new ChirpStyle();
	    }
	    
	    internal ChirpLogger(string name, int id, Color color)
        {
	        Name = name;
	        LoggerId = id;
	        ChannelColor = color;
	        ColorHtml = ColorUtility.ToHtmlStringRGB(ChannelColor);
	        Style = new ChirpStyle();
        }

        private static Color CreateColorHash(string id)
        {
	        var characters = id.ToCharArray();
	        double hash = 0;
	        for (var i = 0; i < characters.Length; i++)
		        hash = char.GetNumericValue(characters[i]) + (((int) hash << 5) - hash);

	        var h = (float) hash % 200;
	        var v = (float) hash % 240;

	        return Color.HSVToRGB(Mathf.Abs(h) / 200f,
		        0.9f,
		        Mathf.Clamp(Mathf.Abs(v) / 240f, 0.7f, 1f));
        }
        
		~ChirpLogger()
		{
			Dispose();
		}
		
		public void Dispose()
		{
		}
    }
 
}