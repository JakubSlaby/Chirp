using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;

namespace WhiteSparrow.Shared.Logging
{
	internal static class ChirpBuildUtils
	{
		private static BuildTargetGroup[] s_BuildTargetGroups;
		
		public static BuildTargetGroup[] GetAvailableBuildTargetGroups()
		{
			if (s_BuildTargetGroups != null)
				return s_BuildTargetGroups;
			
			List<BuildTargetGroup> outputBuildTargetGroups = new List<BuildTargetGroup>();
			var allBuildTargets = Enum.GetValues(typeof(BuildTarget));
			foreach (var buildTarget in allBuildTargets)
			{
				var group = BuildPipeline.GetBuildTargetGroup((BuildTarget)buildTarget);
				if (BuildPipeline.IsBuildTargetSupported(group, (BuildTarget) buildTarget) && !outputBuildTargetGroups.Contains(group))
				{
					
					outputBuildTargetGroups.Add(group);
				}
			}

			s_BuildTargetGroups = outputBuildTargetGroups.ToArray();
			return s_BuildTargetGroups;
		}

		private static string[] s_LogNameByLevelIndex = new string[]
		{
			"Log", "Default", "Info", "Warning", "Assert",
		};

		public static string GetLogNameByLevel(int level)
		{
			if (level < 0 || level >= s_LogNameByLevelIndex.Length)
				return null;
			return s_LogNameByLevelIndex[level];
		}

		public static int GetLogLevelByName(string name)
		{
			return Array.IndexOf(s_LogNameByLevelIndex, name);
		}

		private static Regex s_LogLevelRegex = new Regex("LogLevel(([A-Za-z]+)|([0-9]+))", RegexOptions.Compiled);
		public static int GetLogLevelFromScriptDefines(string scriptDefines)
		{
			var match = s_LogLevelRegex.Match(scriptDefines);
			if (match.Groups[2].Success)
				return GetLogLevelByName(match.Groups[2].Value);
			if (match.Groups[3].Success)
				return Int32.Parse(match.Groups[3].Value);
			return (int)LogLevel.Exception;
		}

		public static void SetTargetGroupChirpEnabled(BuildTargetGroup group, bool enabled)
		{
			string currentScriptDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
			List<string> splitScriptDefines = new List<string>(currentScriptDefines.Split(';'));
			if (enabled && splitScriptDefines.Contains("CHIRP"))
				return;
			if(enabled && !splitScriptDefines.Contains("CHIRP"))
				splitScriptDefines.Add("CHIRP");
			if(!enabled && splitScriptDefines.Contains("CHIRP"))
				splitScriptDefines.Remove("CHIRP");

			string targetScriptDefines = String.Join(";", splitScriptDefines);
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), targetScriptDefines);
		}

		public static void SetTargetGroupLogLevel(BuildTargetGroup group, int level)
		{
			string currentScriptDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group));
			string logLevelString = level == (int) LogLevel.Exception ? "" : $"LogLevel{level}";
			var match = s_LogLevelRegex.Match(currentScriptDefines);
			if (match.Success)
				currentScriptDefines = s_LogLevelRegex.Replace(currentScriptDefines, logLevelString);
			else if(level != (int)LogLevel.Exception)
				currentScriptDefines += $";{logLevelString}";
			
			PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(group), currentScriptDefines);
		}
		
	}
}