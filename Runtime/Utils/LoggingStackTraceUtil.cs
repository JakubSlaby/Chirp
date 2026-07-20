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

		[ThreadStatic]
		private static StringBuilder s_StackTraceBuilderThreadStatic;

		private static string s_ProjectPath;

		private static StringBuilder s_StackTraceBuilder
		{
			get
			{
				if (s_StackTraceBuilderThreadStatic == null)
					s_StackTraceBuilderThreadStatic = new StringBuilder();
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

		// Method → "Namespace.Type:Method(ParamTypes)" fragment. The signature part of a frame
		// never changes, so it is formatted once per distinct method instead of per log call;
		// this also avoids the ParameterInfo[] allocation GetParameters() makes on every call.
		private static readonly ConcurrentDictionary<MethodBase, string> s_MethodSignatureCache = new ConcurrentDictionary<MethodBase, string>();
		private static readonly Func<MethodBase, string> s_BuildMethodSignature = BuildMethodSignature;

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

				var declaringType = method.DeclaringType;
				if (declaringType == null || IsHidden(declaringType, method))
					continue;

				stringBuilder.Append(s_MethodSignatureCache.GetOrAdd(method, s_BuildMethodSignature));

				var str2 = frame.GetFileName();
				if (str2 != null &&
					(!(declaringType.Name == "Debug") || !(declaringType.Namespace == "UnityEngine")) &&
					(!(declaringType.Name == "Logger") || !(declaringType.Namespace == "UnityEngine")) &&
					(!(declaringType.Name == "DebugLogHandler") || !(declaringType.Namespace == "UnityEngine")) &&
					(!(declaringType.Name == "Assert") || !(declaringType.Namespace == "UnityEngine.Assertions")) &&
					(!(method.Name == "print") || !(declaringType.Name == "MonoBehaviour") || !(declaringType.Namespace == "UnityEngine")))
				{
					stringBuilder.Append(" (at ");
					int start = s_ProjectPath != null && str2.StartsWith(s_ProjectPath, StringComparison.OrdinalIgnoreCase)
						? s_ProjectPath.Length
						: 0;
					for (int c = start; c < str2.Length; c++)
					{
						char ch = str2[c];
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
	}
}