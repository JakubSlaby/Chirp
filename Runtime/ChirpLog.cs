using System;
using Newtonsoft.Json;
using WhiteSparrow.Shared.Logging.Core;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging
{
	public class ChirpLog : IChirpLogOptions, IDisposable
	{
		public string Message { get; internal set; }
		
		public LogLevel Level { get; internal set; }
		
		public string StackTrace { get; internal set; }
		public DateTime TimeStamp { get; internal set; }
		public ChirpLogger Source { get; internal set; }

		private WeakReference<Object> m_Context;

		public Object Context
		{
			get => m_Context != null && m_Context.TryGetTarget(out var target) && target != null ? target : null; 
			internal set => m_Context = value !=  null ? new WeakReference<Object>(value) : null;
		}
		
		public IChirpLogOptions Options => this;
		
		internal bool m_HasMarkdown { get; set; }
		bool IChirpLogOptions.HasMarkdown => m_HasMarkdown;
		internal bool m_AddStackTrace { get; set; }
		bool IChirpLogOptions.AddStackTrace => m_AddStackTrace;
		
		internal bool m_IsObjectData { get; set; }
		bool IChirpLogOptions.IsObjectData => m_IsObjectData;

		~ChirpLog() => Dispose();
		
		public void Dispose()
		{
			m_Context = null;
			Message = null;
			Source = null;
			StackTrace = null;
		}
	}

	public interface IChirpLogOptions
	{
		bool HasMarkdown { get; }
		bool AddStackTrace { get; }
		bool IsObjectData { get; }
	}
	

	public static class ChirpLogUtil
	{
		internal static ChirpLog ConstructLog(string message, Object context) => ConstructLog(LogLevel.Log, message, context);
		internal static ChirpLog ConstructLog(LogLevel level, string message, Object context)
		{
			ChirpLog log = new ChirpLog();
			log.Message = message;
			log.Level = level;
			log.Context = context;
			
			return log;
		}

		private static JsonSerializerSettings s_DefaultSerializerSettings = new JsonSerializerSettings()
		{
			Formatting = Formatting.Indented,
			MaxDepth = 1,
			NullValueHandling = NullValueHandling.Include,
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
		};

		public static ChirpLog AsChirpLog(this object instance, JsonSerializerSettings serializerSettings = null) =>
			AsChirpLog(instance, null, serializerSettings);
		public static ChirpLog AsChirpLog(this object instance, string title, JsonSerializerSettings serializerSettings = null)
		{
			try
			{
				string json = JsonConvert.SerializeObject(instance, serializerSettings ?? s_DefaultSerializerSettings);
				ChirpLog log = ConstructLog($"```{title ?? instance.GetType().Name}\r\n{json}\r\n```", instance as Object);
				log.m_IsObjectData = true;
				log.m_HasMarkdown = true;
				return log;
			}
			catch (Exception e)
			{
				return AsChirpLog($"{e.Message}\r\n{e.StackTrace}");
			}
		}
		
		public static ChirpLog AsChirpLog(this string message)
		{
			return ConstructLog(message, null);
		}

		public static ChirpLog AddStackTrace(this ChirpLog log)
		{
			log.m_AddStackTrace = true;
			return log;
		}

		public static ChirpLog AsMarkdownLog(this string message)
		{
			var log = AsChirpLog(message);
			log.m_HasMarkdown = true;
			return log;
		}
		public static ChirpLog AsMarkdown(this ChirpLog log)
		{
			log.m_HasMarkdown = true;
			return log;
		}

		public static ChirpLog WithContext(this ChirpLog log, Object context)
		{
			log.Context = context;
			return log;
		}
		
	}

}