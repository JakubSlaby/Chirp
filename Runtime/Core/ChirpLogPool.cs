using System;
using System.Collections.Generic;
using System.Threading;

namespace WhiteSparrow.Shared.Logging.Core
{
	/// <summary>
	/// Per-thread pool of ChirpLog instances. Logs may be constructed and submitted from any
	/// thread — main thread, System.Threading.Tasks thread pool, Unity job workers, UniTask
	/// continuations or raw threads. [ThreadStatic] storage keeps every path lock-free; when a
	/// log is constructed on one thread and submitted on another it simply migrates to the
	/// submitting thread's pool, which is correct and merely trades a little reuse.
	/// </summary>
	internal static class ChirpLogPool
	{
		private const int k_MaxPoolSize = 32;

		[ThreadStatic]
		private static Stack<ChirpLog> s_Pool;

		internal static ChirpLog Rent()
		{
			var pool = s_Pool;
			if (pool == null || !pool.TryPop(out var log))
				log = new ChirpLog();

			log.m_ReleasedFlag = 0;
			log.m_Pooled = true;
			unchecked
			{
				log.m_Generation++;
			}

			return log;
		}

		internal static void Return(ChirpLog log)
		{
			if (log == null)
				return;

			// Idempotent, and safe against a concurrent double-Dispose: only the first caller
			// proceeds. A double return would hand the same instance out to two logs at once.
			if (Interlocked.Exchange(ref log.m_ReleasedFlag, 1) != 0)
				return;

			log.Reset();

			if (!log.m_Pooled)
				return;

			var pool = s_Pool ??= new Stack<ChirpLog>(k_MaxPoolSize);
			if (pool.Count < k_MaxPoolSize)
				pool.Push(log);
		}
	}
}
