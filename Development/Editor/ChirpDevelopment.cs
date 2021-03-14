using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	public static class ChirpDevelopment
	{
		private static string s_RootFilePathCache;

		private static string s_ChirpRepositoryRootCache;

		private static DirectoryInfo s_DataPathDirectoryCache;

		private static string RootFilePath
		{
			get
			{
				if (s_RootFilePathCache == null)
				{
					var search = AssetDatabase.FindAssets("ChirpDevelopmentRoot");
					if (search.Length == 0)
						throw new Exception(
							"Unable to find ChirpDevelopmentRoot file. The repository structure is corrupted.");
					if (search.Length > 1)
						throw new Exception(
							"Found more than one ChirpDevelopmentRoot file. The repository structure is corrupted.");
					var rootFile = new FileInfo(AssetDatabase.GUIDToAssetPath(search[0]));
					s_RootFilePathCache = rootFile.Directory.FullName;
				}

				return s_RootFilePathCache;
			}
		}

		public static string ChirpRepositoryRoot
		{
			get
			{
				if (s_ChirpRepositoryRootCache == null) s_ChirpRepositoryRootCache = RootFilePath + "/../../";

				return s_ChirpRepositoryRootCache;
			}
		}

		private static DirectoryInfo DataPathDirectory
		{
			get
			{
				if (s_DataPathDirectoryCache == null)
					s_DataPathDirectoryCache = new DirectoryInfo(Application.dataPath);
				return s_DataPathDirectoryCache;
			}
		}

		public static string MakePathRelative(string input)
		{
			var l = 0;
			if (input.IndexOf(Path.DirectorySeparatorChar) != -1)
			{
				if (input.IndexOf(DataPathDirectory.FullName, StringComparison.Ordinal) == 0)
					l = DataPathDirectory.FullName.Length;
			}
			else
			{
				if (input.IndexOf(Application.dataPath, StringComparison.Ordinal) == 0)
					l = Application.dataPath.Length;
			}

			if (l == 0)
				return input;


			var output = "Assets" + input.Substring(l, input.Length - l);
			return output.Replace("\\", "/").TrimEnd('/');
		}
	}
}