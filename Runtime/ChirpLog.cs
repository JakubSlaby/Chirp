using System;
using System.Diagnostics;
using Newtonsoft.Json;
using WhiteSparrow.Shared.Logging.Core;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging
{
	/// <summary>
	/// A single in-flight log event. Instances are pooled: they are rented when the log is
	/// constructed and returned to the pool as soon as ChirpImpl.Submit has dispatched them to
	/// all outputs. IChirpOutput implementations must therefore not retain a ChirpLog beyond
	/// Ingest() — use <see cref="Copy"/> to keep a caller-owned snapshot instead.
	/// </summary>
	public class ChirpLog : IChirpLogOptions, IDisposable
	{
		private string m_Message;
		private LogLevel m_Level;
		private string m_StackTrace;
		private DateTime m_TimeStamp;
		private ChirpLogger m_Source;
		private Object m_Context;

		// 0 = live, 1 = released back to the pool. Kept as an int so ChirpLogPool.Return can
		// guard idempotently with Interlocked.Exchange.
		internal int m_ReleasedFlag;
		internal bool m_Pooled;
		internal int m_Generation;
#if UNITY_EDITOR
		private int m_ReleasedAccessWarningGeneration = -1;
#endif

		public string Message
		{
			get
			{
				GuardReleased();
				return m_Message;
			}
			internal set
			{
				GuardReleased();
				m_Message = value;
			}
		}

		public LogLevel Level
		{
			get
			{
				GuardReleased();
				return m_Level;
			}
			internal set
			{
				GuardReleased();
				m_Level = value;
			}
		}

		public string StackTrace
		{
			get
			{
				GuardReleased();
				return m_StackTrace;
			}
			internal set
			{
				GuardReleased();
				m_StackTrace = value;
			}
		}

		public DateTime TimeStamp
		{
			get
			{
				GuardReleased();
				return m_TimeStamp;
			}
			internal set
			{
				GuardReleased();
				m_TimeStamp = value;
			}
		}

		public ChirpLogger Source
		{
			get
			{
				GuardReleased();
				return m_Source;
			}
			internal set
			{
				GuardReleased();
				m_Source = value;
			}
		}

		public Object Context
		{
			get
			{
				GuardReleased();
				// The log only lives for the duration of a synchronous Submit, so a strong
				// reference cannot root the object for long. Unity's overloaded != treats
				// destroyed objects as null — normalise those back to a true null reference.
				return m_Context != null ? m_Context : null;
			}
			internal set
			{
				GuardReleased();
				m_Context = value;
			}
		}

		public IChirpLogOptions Options => this;

		internal bool m_HasMarkdown { get; set; }
		bool IChirpLogOptions.HasMarkdown => m_HasMarkdown;
		internal bool m_AddStackTrace { get; set; }
		bool IChirpLogOptions.AddStackTrace => m_AddStackTrace;

		internal bool m_IsObjectData { get; set; }
		bool IChirpLogOptions.IsObjectData => m_IsObjectData;

		/// <summary>
		/// Creates a non-pooled snapshot of this log that the caller owns. This is the
		/// sanctioned way for buffering outputs (file writers, network batches) to keep log
		/// data beyond the duration of Ingest().
		/// </summary>
		public ChirpLog Copy()
		{
			GuardReleased();
			var copy = new ChirpLog();
			copy.m_Message = m_Message;
			copy.m_Level = m_Level;
			copy.m_StackTrace = m_StackTrace;
			copy.m_TimeStamp = m_TimeStamp;
			copy.m_Source = m_Source;
			copy.m_Context = m_Context;
			copy.m_HasMarkdown = m_HasMarkdown;
			copy.m_AddStackTrace = m_AddStackTrace;
			copy.m_IsObjectData = m_IsObjectData;
			return copy;
		}

		internal void Reset()
		{
			m_Message = null;
			m_Level = default;
			m_StackTrace = null;
			m_TimeStamp = default;
			m_Source = null;
			m_Context = null;
			m_HasMarkdown = false;
			m_AddStackTrace = false;
			m_IsObjectData = false;
		}

		public void Dispose()
		{
			ChirpLogPool.Return(this);
		}

		[Conditional("UNITY_EDITOR")]
		private void GuardReleased()
		{
#if UNITY_EDITOR
			if (m_ReleasedFlag == 0 || m_ReleasedAccessWarningGeneration == m_Generation)
				return;
			m_ReleasedAccessWarningGeneration = m_Generation;
			UnityEngine.Debug.LogError("[Chirp] ChirpLog was accessed after it was released back to the pool. ChirpLog instances are only valid until Submit returns — IChirpOutput implementations must not retain them beyond Ingest(). Use ChirpLog.Copy() to keep a snapshot.");
#endif
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
			ChirpLog log = ChirpLogPool.Rent();
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
				ChirpLog log = ConstructLog(string.Concat("```", title ?? instance.GetType().Name, "\r\n", json, "\r\n```"), instance as Object);
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
