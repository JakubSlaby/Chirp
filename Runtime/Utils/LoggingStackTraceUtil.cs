using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	public static class LoggingStackTraceUtil
	{
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

		public static string FormatUnityStackTrace(StackTrace stackTrace)
		{
			// channel = null;
			var stringBuilder = s_StackTraceBuilder;
			stringBuilder.Clear();
			for (var index1 = 0; index1 < stackTrace.FrameCount; ++index1)
			{
				var frame = stackTrace.GetFrame(index1);
				var method = frame.GetMethod();
				if (method != null)
				{
					var declaringType = method.DeclaringType;
					if (declaringType != null)
					{
						// if(channel == null)
						// 	channel = LogChannel.GetForTarget(declaringType);
						
						var str1 = declaringType.Namespace;
						if (!string.IsNullOrEmpty(str1))
						{
							stringBuilder.Append(str1);
							stringBuilder.Append(".");
						}

						stringBuilder.Append(declaringType.Name);
						stringBuilder.Append(":");
						stringBuilder.Append(method.Name);
						stringBuilder.Append("(");
						var index2 = 0;
						var parameters = method.GetParameters();
						var flag = true;
						for (; index2 < parameters.Length; ++index2)
						{
							if (!flag)
								stringBuilder.Append(", ");
							else
								flag = false;
							stringBuilder.Append(parameters[index2].ParameterType.Name);
						}

						stringBuilder.Append(")");
						var str2 = frame.GetFileName();
						if (str2 != null &&
							(!(declaringType.Name == "Debug") || !(declaringType.Namespace == "UnityEngine")) &&
							(!(declaringType.Name == "Logger") || !(declaringType.Namespace == "UnityEngine")) &&
							(!(declaringType.Name == "DebugLogHandler") || !(declaringType.Namespace == "UnityEngine")) &&
							(!(declaringType.Name == "Assert") || !(declaringType.Namespace == "UnityEngine.Assertions")) &&
							(!(method.Name == "print") || !(declaringType.Name == "MonoBehaviour") || !(declaringType.Namespace == "UnityEngine")))
						{
							stringBuilder.Append(" (at ");
							if (s_ProjectPath != null)
							{
								if (str2.StartsWith(s_ProjectPath))
									str2 = str2.Substring(s_ProjectPath.Length);
								stringBuilder.Append(str2);
							}
							stringBuilder.Append(":");
							stringBuilder.Append(frame.GetFileLineNumber().ToString());
							stringBuilder.Append(")");
						}

						stringBuilder.Append("\n");
					}
				}
			}

			return stringBuilder.ToString().Trim('\n', '\r');
		}
	}
}