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

		internal static long MeasureTotalBytes(Action action)
		{
			for (int i = 0; i < k_WarmupIterations; i++)
				action();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			long before = GC.GetTotalMemory(false);
			for (int i = 0; i < k_MeasuredIterations; i++)
				action();
			long after = GC.GetTotalMemory(false);
			return Math.Max(0, after - before);
		}

		internal static void ReportAndAssert(string label, long totalBytes, long perCallBudget)
		{
			double perCall = totalBytes / (double)k_MeasuredIterations;
			TestContext.WriteLine("{0}: {1:F1} B/call ({2} B over {3} calls)", label, perCall, totalBytes, k_MeasuredIterations);
			Assert.LessOrEqual(totalBytes, perCallBudget * k_MeasuredIterations,
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
}
