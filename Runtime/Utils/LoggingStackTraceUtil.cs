using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Unity.Profiling;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	public static class LoggingStackTraceUtil
	{
		private const string k_HiddenNamespace = "WhiteSparrow.Shared.Logging";

		private static readonly ProfilerMarker s_CaptureStackTraceMarker = new ProfilerMarker("LoggingStackTraceUtil.CaptureStackTrace");
		private static readonly ProfilerMarker s_FormatUnityStackTraceMarker = new ProfilerMarker("LoggingStackTraceUtil.FormatUnityStackTrace");

		// Sized to hold a typical formatted trace in a single chunk. StringBuilder.Clear() only
		// takes its allocation-free fast path while the builder is still one chunk — once it has
		// spilled, every Clear() allocates a fresh array to collapse the chain back down, on
		// every call. One 4 KB array per thread up front buys that away for good.
		private const int k_StackTraceBuilderCapacity = 2048;

		[ThreadStatic]
		private static StringBuilder s_StackTraceBuilderThreadStatic;

		private static string s_ProjectPath;

		private static StringBuilder s_StackTraceBuilder
		{
			get
			{
				if (s_StackTraceBuilderThreadStatic == null)
					s_StackTraceBuilderThreadStatic = new StringBuilder(k_StackTraceBuilderCapacity);
				return s_StackTraceBuilderThreadStatic;
			}
		}

		[RuntimeInitializeOnLoadMethod]
		private static void InitializeProjectPath()
		{
			s_ProjectPath = Application.dataPath;
			var i = s_ProjectPath.LastIndexOf("Assets", StringComparison.Ordinal);
			if (i != -1)
				s_ProjectPath = s_ProjectPath.Substring(0, i);
			s_ProjectPath = s_ProjectPath.Replace('/', Path.DirectorySeparatorChar);
		}
		
		public static string ExtractStackTrace()
		{
			using var _ = s_CaptureStackTraceMarker.Auto();
			var stackTrace = CaptureStackTrace();
			return FormatUnityStackTrace(stackTrace);
		}

		[HideInCallstack]
		public static StackTrace CaptureStackTrace()
		{
			using var _ = s_CaptureStackTraceMarker.Auto();
			return new StackTrace(true);
		}

		/// <summary>
		/// Everything about a frame that depends only on its method, and therefore never changes
		/// between log calls: the rendered signature, whether the frame is hidden, and whether it
		/// is one of the Unity entry points that should print without a file/line suffix.
		/// </summary>
		private sealed class FrameFormat
		{
			public readonly string Signature;
			public readonly bool Hidden;
			public readonly bool AllowFileInfo;

			public 
        (string signature, bool hidden, bool allowFileInfo)
			{
				Signature = signature;
				Hidden = hidden;
				AllowFileInfo = allowFileInfo;
			}
		}

		// Method → formatting decisions, resolved once per distinct method rather than per log
		// call. This keeps three things off the hot path: the ParameterInfo[] that GetParameters()
		// allocates, the custom-attribute walk behind IsDefined(HideInCallstack), and the chain of
		// namespace/type-name string comparisons used to spot Unity's own logging entry points.
		private static readonly ConcurrentDictionary<MethodBase, FrameFormat> s_FrameFormatCache = new ConcurrentDictionary<MethodBase, FrameFormat>();
		private static readonly Func<MethodBase, FrameFormat> s_BuildFrameFormat = BuildFrameFormat;

		public static string FormatUnityStackTrace(StackTrace stackTrace)
		{
			using var _ = s_FormatUnityStackTraceMarker.Auto();

			var stringBuilder = s_StackTraceBuilder;
			stringBuilder.Clear();
			for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1)
			{
				var frame = stackTrace.GetFrame(index1);
				var method = frame.GetMethod();
				if (method == null)
					continue;

				var format = s_FrameFormatCache.GetOrAdd(method, s_BuildFrameFormat);
				if (format.Hidden)
					continue;

				stringBuilder.Append(format.Signature);

				var fileName = frame.GetFileName();
				if (fileName != null && format.AllowFileInfo)
				{
					stringBuilder.Append(" (at ");
					int start = s_ProjectPath != null && fileName.StartsWith(s_ProjectPath, StringComparison.OrdinalIgnoreCase)
						? s_ProjectPath.Length
						: 0;
					for (int c = start; c < fileName.Length; c++)
					{
						char ch = fileName[c];
						stringBuilder.Append(ch == '\\' ? '/' : ch);
					}

					stringBuilder.Append(':');
					StringBuilderUtil.AppendInt(stringBuilder, frame.GetFileLineNumber());
					stringBuilder.Append(')');
				}

				stringBuilder.Append('\n');
			}

			StringBuilderUtil.TrimEndLineBreaks(stringBuilder);
			return stringBuilder.ToString();
		}

		private static readonly FrameFormat s_HiddenFrame = new FrameFormat(null, true, false);

		private static FrameFormat BuildFrameFormat(MethodBase method)
		{
			var declaringType = method.DeclaringType;
			if (declaringType == null || IsHidden(declaringType, method))
				return s_HiddenFrame;

			return new FrameFormat(BuildMethodSignature(method), false, AllowsFileInfo(declaringType, method));
		}

		// Mirrors Unity's own StackTraceUtility: its logging entry points are named in the trace
		// but never given a file/line suffix, so a double-click lands on the caller instead.
		private static bool AllowsFileInfo(Type declaringType, MethodBase method)
		{
			var ns = declaringType.Namespace;
			var name = declaringType.Name;

			if (ns == "UnityEngine")
				return name != "Debug"
					&& name != "Logger"
					&& name != "DebugLogHandler"
					&& !(name == "MonoBehaviour" && method.Name == "print");

			if (ns == "UnityEngine.Assertions")
				return name != "Assert";

			return true;
		}

		private static string BuildMethodSignature(MethodBase method)
		{
			var sb = new StringBuilder();
			var declaringType = method.DeclaringType;
			var ns = declaringType?.Namespace;
			if (!string.IsNullOrEmpty(ns))
			{
				sb.Append(ns);
				sb.Append('.');
			}

			sb.Append(declaringType?.Name);
			sb.Append(':');
			sb.Append(method.Name);
			sb.Append('(');
			var parameters = method.GetParameters();
			for (var i = 0; i < parameters.Length; i++)
			{
				if (i > 0)
					sb.Append(", ");
				sb.Append(parameters[i].ParameterType.Name);
			}

			sb.Append(')');
			return sb.ToString();
		}

		private static bool IsHidden(Type declaringType, System.Reflection.MethodBase method)
		{
			var ns = declaringType.Namespace;
			if (ns != null && (ns == k_HiddenNamespace || ns.StartsWith(k_HiddenNamespace + ".", StringComparison.Ordinal)))
				return true;

			return method.IsDefined(typeof(HideInCallstackAttribute), false);
		}

		private static bool IsHidden(Type declaringType, System.Reflection.MethodBase method)
		{
			var ns = declaringType.Namespace;
			if (ns != null && (ns == k_HiddenNamespace || ns.StartsWith(k_HiddenNamespace + ".", StringComparison.Ordinal)))
				return true;

			return method.IsDefined(typeof(HideInCallstackAttribute), false);
		}
	}
}