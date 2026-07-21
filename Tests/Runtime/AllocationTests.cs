using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using WhiteSparrow.Shared.Logging.Core;
using WhiteSparrow.Shared.Logging.Outputs;
using Is = UnityEngine.TestTools.Constraints.Is;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging.Tests
{
	internal class ChirpTestContextObject : ScriptableObject
	{
	}

	/// <summary>
	/// Verifies the allocation behaviour of the logging hot path.
	/// Zero-allocation shapes are asserted with Unity's AllocatingGCMemory constraint, which
	/// counts GC.Alloc profiler samples during the delegate — the canonical Unity way to prove
	/// a code path does not allocate. (GC.GetAllocatedBytesForCurrentThread is not functional
	/// on Unity's Mono runtime, so byte-exact per-thread measurement is unavailable here.)
	/// Allocation-heavy shapes are budgeted via GC.GetTotalMemory deltas; those numbers are
	/// indicative (a GC run mid-measurement undercounts) — record the printed B/call in
	/// Documentation~/string-optimisation-followup.md.
	/// Uses the internal (non-[Conditional]) pipeline entry points so the measurements do not
	/// depend on the CHIRP scripting define.
	/// </summary>
	public class AllocationTests
	{
		private const int k_WarmupIterations = 200;
		private const int k_ZeroCheckIterations = 100;
		private const int k_MeasuredIterations = 100;

		// Byte budgets per call for the allocation-heavy shapes. Regression canaries, not
		// targets — tighten them once the printed Unity/Mono numbers are recorded.
		// Markdown parse: regex Match/Group machinery dominates (~6.4 KB/call measured on
		// CoreCLR for a three-element message).
		private const long k_MarkdownParseBudget = 16 * 1024;
		// StackTrace(true) with file info is inherently allocation-heavy.
		private const long k_StackTraceLogBudget = 64 * 1024;

		private class NoOpOutput : AbstractChirpOutput
		{
			protected override void OnInitialize()
			{
			}

			protected override bool Filter(ChirpLog logEvent) => true;

			protected override void Process(ChirpLog logEvent)
			{
				// Touch the fields a real output would read.
				Consume(logEvent.Message);
				Consume(logEvent.Source);
				Consume(logEvent.Context);
			}

			private static void Consume(object value)
			{
			}
		}

		private NoOpOutput m_Output;
		private ChirpLogger m_Channel;
		private ChirpTestContextObject m_Context;
		private IChirpPlugin[] m_DetachedPlugins;

		[SetUp]
		public void SetUp()
		{
			// These tests run in PlayMode inside a host project, which may already have outputs
			// registered (a ChirpInitialize with a UnityConsoleLogger, for instance). Those would
			// format and print every submitted log, so the measurement would be of the host's
			// console output rather than of the pipeline. Take the pipeline over for the test.
			m_DetachedPlugins = Chirp.Impl.DetachAllPlugins();

			m_Output = new NoOpOutput();
			Chirp.AddPlugin(m_Output);
			m_Channel = new ChirpLogger("AllocTest");
			m_Context = ScriptableObject.CreateInstance<ChirpTestContextObject>();
		}

		[TearDown]
		public void TearDown()
		{
			Chirp.RemovePlugin(m_Output);
			Chirp.Impl.RestorePlugins(m_DetachedPlugins);
			m_DetachedPlugins = null;

			if (m_Context != null)
				Object.DestroyImmediate(m_Context);
		}

		[Test]
		public void PlainLog_IsAllocationFree()
		{
			AssertAllocationFree("Plain log (no-op output)",
				() => SubmitLog(Chirp.Logger, "Constant plain log message", null, false, false));
		}

		[Test]
		public void ChannelLog_IsAllocationFree()
		{
			AssertAllocationFree("Channel log (no-op output)",
				() => SubmitLog(m_Channel, "Constant channel log message", null, false, false));
		}

		[Test]
		public void ContextLog_IsAllocationFree()
		{
			AssertAllocationFree("Context log (no-op output)",
				() => SubmitLog(Chirp.Logger, "Constant context log message", m_Context, false, false));
		}

		[Test]
		public void MarkdownLogWithoutMarkdownOutput_IsAllocationFree()
		{
			// The markdown flag alone costs nothing — parsing happens in outputs that render it.
			AssertAllocationFree("Markdown-flagged log (no-op output)",
				() => SubmitLog(Chirp.Logger, "Constant **markdown** message", null, true, false));
		}

		[Test]
		public void MarkdownParse_PlainText_IsAllocationFree()
		{
			var style = Chirp.Style;
			AssertAllocationFree("Markdown parse of plain text",
				() => LoggingMarkdownUtil.Parse("Message without any markdown elements at all", style));
		}

		[Test]
		public void MarkdownParse_StaysWithinBudget()
		{
			var style = Chirp.Style;
			long total = MeasureTotalBytes(() => LoggingMarkdownUtil.Parse("Message with **bold**, *italic* and `inline code` elements", style));
			ReportAndAssert("Markdown parse", total, k_MarkdownParseBudget);
		}

		[Test]
		public void StackTraceLog_StaysWithinBudget()
		{
			long total = MeasureTotalBytes(() => SubmitLog(Chirp.Logger, "Constant stack trace message", null, false, true));
			ReportAndAssert("Stack-trace log (no-op output)", total, k_StackTraceLogBudget);
		}

		private static void SubmitLog(ChirpLogger source, string message, Object context, bool markdown, bool stackTrace)
		{
			var log = ChirpLogUtil.ConstructLog(message, context);
			log.Source = source;
			log.Level = LogLevel.Log;
			if (markdown)
				log.m_HasMarkdown = true;
			if (stackTrace)
				log.m_AddStackTrace = true;
			Chirp.Impl.Submit(log);
		}

		internal static void AssertAllocationFree(string label, Action action)
		{
			for (int i = 0; i < k_WarmupIterations; i++)
				action();

			Assert.That(() =>
			{
				for (int i = 0; i < k_ZeroCheckIterations; i++)
					action();
			}, Is.Not.AllocatingGCMemory(), label + " should not allocate GC memory");
		}

		internal static long MeasureTotalBytes(Action action) => MeasureTotalBytes(action, k_MeasuredIterations);

		/// <summary>
		/// Total bytes allocated across <paramref name="iterations"/> calls, measured over a
		/// window in which no garbage collection ran.
		/// </summary>
		/// <remarks>
		/// A collection inside the measurement window reclaims part of what the window just
		/// allocated, so the GetTotalMemory delta under-reports — which for a budget assertion
		/// means passing for the wrong reason, the one failure mode a regression canary must not
		/// have. Rather than clamp the delta and hope, the window is retried until one completes
		/// cleanly, and the test is marked inconclusive if none does. Pick an iteration count
		/// whose total stays inside the nursery: too few and the per-call figure drowns in
		/// measurement noise, too many and a collection is guaranteed.
		/// </remarks>
		internal static long MeasureTotalBytes(Action action, int iterations)
		{
			const int k_MaxAttempts = 5;

			for (int i = 0; i < k_WarmupIterations; i++)
				action();

			for (int attempt = 0; attempt < k_MaxAttempts; attempt++)
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
				GC.Collect();

				int collectionsBefore = GC.CollectionCount(0);
				long before = GC.GetTotalMemory(false);
				for (int i = 0; i < iterations; i++)
					action();
				long after = GC.GetTotalMemory(false);

				if (GC.CollectionCount(0) == collectionsBefore)
					return Math.Max(0, after - before);
			}

			Assert.Inconclusive("Could not complete {0} iterations without a garbage collection, so the allocation figure would under-report. Lower the iteration count for this measurement.", iterations);
			return 0;
		}

		internal static void ReportAndAssert(string label, long totalBytes, long perCallBudget)
			=> ReportAndAssert(label, totalBytes, perCallBudget, k_MeasuredIterations);

		internal static void ReportAndAssert(string label, long totalBytes, long perCallBudget, int iterations)
		{
			double perCall = totalBytes / (double)iterations;
			TestContext.WriteLine("{0}: {1:F1} B/call ({2} B over {3} calls)", label, perCall, totalBytes, iterations);
			Assert.LessOrEqual(totalBytes, perCallBudget * iterations,
				"{0} exceeded the budget of {1} B/call: measured {2:F1} B/call", label, perCallBudget, perCall);
		}
	}

	/// <summary>
	/// End-to-end measurement through UnityConsolePlugin. This includes Unity's own managed
	/// work (its internal string.Format copy on the default path, plus whatever the editor
	/// console and the test framework log hooks allocate), so the budget is intentionally
	/// loose — the tight zero-allocation assertions live in AllocationTests.
	/// </summary>
	public class ConsoleAllocationTests
	{
		private const long k_ConsoleLogBudget = 8 * 1024;

		private UnityConsolePlugin m_Console;
		private IChirpPlugin[] m_DetachedPlugins;

		[SetUp]
		public void SetUp()
		{
			// As in AllocationTests: measure exactly one console plugin, not this one plus
			// whatever the host project already had registered.
			m_DetachedPlugins = Chirp.Impl.DetachAllPlugins();
			m_Console = Chirp.AddPlugin<UnityConsolePlugin>();
		}

		[TearDown]
		public void TearDown()
		{
			m_Console.Dispose();
			Chirp.Impl.RestorePlugins(m_DetachedPlugins);
			m_DetachedPlugins = null;
		}

		[Test]
		public void ConsoleLog_StaysWithinBudget()
		{
			long total = AllocationTests.MeasureTotalBytes(() =>
			{
				var log = ChirpLogUtil.ConstructLog("Constant console log message", null);
				log.Source = Chirp.Logger;
				log.Level = LogLevel.Log;
				Chirp.Impl.Submit(log);
			});
			AllocationTests.ReportAndAssert("Console log (end-to-end)", total, k_ConsoleLogBudget);
		}
	}

	/// <summary>
	/// Budgets UnityConsolePlugin's own formatting step. This is deliberately not measured through
	/// Process: that hands the formatted string to Unity's console, which retains it, and under the
	/// test runner the framework's log capture allocates per message as well. Neither is Chirp's
	/// cost, and both would swamp a hundred-byte budget. What is budgeted here is what the plugin
	/// actually controls — building the console line and materialising it as a string.
	///
	/// The expected shape is a single string.  Everything upstream of the ToString is reused:
	/// the ChannelPrefix is precomputed on ChirpLogger, and the StringBuilder is thread-static and
	/// pre-sized so Clear() keeps its single-chunk fast path.
	///
	/// FormatConsoleLog reads only the log it is handed, so the plugin here is constructed directly
	/// rather than registered with Chirp — nothing needs the receiver, the channel, or the Unity
	/// log-handler takeover, and staying out of the pipeline keeps the fixture free of global side
	/// effects.
	/// </summary>
	public class UnityConsoleFormatAllocationTests
	{
		// One string of (prefix + message) chars. A char is two bytes and a string header is ~22,
		// so 100 bytes buys roughly 39 characters of console line — the budget is really an
		// assertion that the format path produces exactly one string and nothing else, pinned
		// against a representative short line. A longer message legitimately costs more; that is
		// the caller's string, not overhead. Hence the unchannelled logger below: the channel
		// prefix alone ("[<color=#RRGGBB>Name</color>] ") is 35 characters and would consume the
		// whole budget on its own. The channel path is covered by the stack-trace test, which has
		// the headroom to carry it.
		private const long k_FormatBudget = 100;

		// One string of (message + newline + trace) chars. Sized for a trace from a normal game
		// call site — see s_RepresentativeStackTrace.
		private const long k_FormatWithStackTraceBudget = 2 * 1024;

		// Tight budgets need a long window to rise above GetTotalMemory noise; the stack-trace
		// window allocates ~20x more per call, so it runs shorter to stay inside the nursery.
		private const int k_FormatIterations = 2000;
		private const int k_FormatWithStackTraceIterations = 500;

		// A real captured trace is not usable as a fixture here: under NUnit every capture carries
		// ~40 frames of test-runner machinery below the call site, several times deeper than any
		// game call site, which would make the measurement a function of the runner rather than of
		// the plugin. This is a representative trace from an ordinary call depth instead, so the
		// budget means the same thing on every machine and in every runner.
		private static readonly string s_RepresentativeStackTrace = string.Join("\n", new[]
		{
			"Nightjar.Gameplay.PlayerController:HandleInput(InputFrame) (at Assets/Scripts/Gameplay/PlayerController.cs:184)",
			"Nightjar.Gameplay.PlayerController:Tick(Single) (at Assets/Scripts/Gameplay/PlayerController.cs:92)",
			"Nightjar.Core.SystemScheduler:RunTick(Single) (at Assets/Scripts/Core/SystemScheduler.cs:216)",
			"Nightjar.Core.GameLoop:Update() (at Assets/Scripts/Core/GameLoop.cs:57)",
		});

		private UnityConsolePlugin m_Console;
		private ChirpLogger m_Channel;
		private ChirpLog m_Log;

		[SetUp]
		public void SetUp()
		{
			m_Console = new UnityConsolePlugin();
			m_Channel = new ChirpLogger("AllocTest");
		}

		[TearDown]
		public void TearDown()
		{
			// Returned here rather than at the end of the test so a failed assertion still hands
			// the instance back to the pool.
			m_Log?.Dispose();
			m_Log = null;
		}

		[Test]
		public void FormatConsoleLog_WithoutStackTrace_StaysWithinBudget()
		{
			// Chirp.Logger is the default channel (UseChannel: false), so the formatted line is
			// the message and nothing else — 28 chars, ~78 bytes as a string.
			m_Log = ChirpLogUtil.ConstructLog("Constant console log message", null);
			m_Log.Source = Chirp.Logger;
			m_Log.Level = LogLevel.Log;

			long total = AllocationTests.MeasureTotalBytes(
				() => m_Console.FormatConsoleLog(m_Log, false), k_FormatIterations);

			AllocationTests.ReportAndAssert("Console format (no stack trace)", total, k_FormatBudget, k_FormatIterations);
		}

		[Test]
		public void FormatConsoleLog_WithStackTrace_StaysWithinBudget()
		{
			m_Log = ChirpLogUtil.ConstructLog("Constant console log message", null);
			m_Log.Source = m_Channel;
			m_Log.Level = LogLevel.Log;
			m_Log.StackTrace = s_RepresentativeStackTrace;

			long total = AllocationTests.MeasureTotalBytes(
				() => m_Console.FormatConsoleLog(m_Log, true), k_FormatWithStackTraceIterations);

			AllocationTests.ReportAndAssert("Console format (with stack trace)", total, k_FormatWithStackTraceBudget, k_FormatWithStackTraceIterations);
		}
	}
}
