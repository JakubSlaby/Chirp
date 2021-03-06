using System;
using System.Collections.Generic;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{

	public class LogChannel : IEquatable<LogChannel>, IEquatable<string>
	{
		public readonly string id;
		public readonly string name;
		public readonly Color color;
		
		public LogChannel(string id) : this(id, CreateColorHash(id))
		{
			
		}

		public LogChannel(string id, Color color)
		{
			this.id = id.ToLower();
			this.name = id;
			this.color = color;

			RegisterChannel(this);
		}

		private static Color CreateColorHash(string id)
		{
			char[] characters = id.ToCharArray();
			double hash = 0;
			for (int i = 0; i < characters.Length; i++)
			{
				hash = char.GetNumericValue(characters[i]) + (((int)hash << 5) - hash);
			}

			float h = (float)hash % 200;
			float v = (float)hash % 240;
			
			return Color.HSVToRGB(Mathf.Abs(h)/200f, 
								  0.9f, 
								  Mathf.Clamp(Mathf.Abs(v)/240f, 0.7f, 1f));
		}
	
		// custom operator to use string id for easy assignment
		public static implicit operator LogChannel(string logChannel)
		{
			return Get(logChannel);
		}

		#region Comparison

		public bool Equals(LogChannel other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return id == other.id;
		}

		public bool Equals(string other)
		{
			return other != null && id == other.ToLower();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj is string channelId)
				return Equals(channelId);
			if (obj is LogChannel channel)
				return Equals(channel);
			return false;
		}

		public override int GetHashCode()
		{
			return (id != null ? id.GetHashCode() : 0);
		}

		#endregion

		#region Static Registry

		private static Dictionary<string, LogChannel> s_IdToInstanceMapping = new Dictionary<string, LogChannel>();
		private static List<string> s_IdList = new List<string>();
		private static string[] s_IdListCache;

		private void RegisterChannel(LogChannel channel)
		{
			if (s_IdToInstanceMapping.ContainsKey(channel.id))
				return;

			s_IdToInstanceMapping.Add(channel.id, channel);
			s_IdList.Add(channel.id);
		}

		public static LogChannel Get(string id)
		{
			if (s_IdToInstanceMapping.TryGetValue(id, out var channel))
				return channel;
			
			return new LogChannel(id);
		}

		public static string[] GetAllChannelIds()
		{
			if (s_IdListCache == null)
				s_IdListCache = s_IdList.ToArray();
			return s_IdListCache;
		}

		public static bool HasChannel(string id)
		{
			return s_IdList.Contains(id);
		}
		
		#endregion

	}
	
	
}