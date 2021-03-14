using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	public static class ExportPackage
	{
		private const string s_ContentDirectory = "WhiteSparrow/Logging";
		private const string s_ExportDirectory = "Plugins/WhiteSparrow/Logging";


		[MenuItem("Tools/Chirp Logger/Export Package", priority = 600)]
		public static void ExportChirpPackage()
		{
			var rootDirectory = new DirectoryInfo(ChirpDevelopment.ChirpRepositoryRoot);
			var contentDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, s_ContentDirectory));
			var targetDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, s_ExportDirectory));
			if (targetDirectory.Exists)
			{
				EditorUtility.DisplayDialog("Chirp: Exporting Package",
					$"The target packaging directory is already created - unable to move the files for packaging.\npath: {targetDirectory.FullName}",
					"Ok");
				return;
			}

			if (targetDirectory.Parent != null && !targetDirectory.Parent.Exists)
			{
				Directory.CreateDirectory(targetDirectory.Parent.FullName);
			}
			

			var from = ChirpDevelopment.MakePathRelative(contentDirectory.FullName);
			var to = ChirpDevelopment.MakePathRelative(targetDirectory.FullName);
			FileUtil.MoveFileOrDirectory(from, to);
			AssetDatabase.Refresh();

			var files = AssetDatabase.FindAssets("*", new[] {to.TrimEnd('/')});
			if (files.Length == 0)
			{
				EditorUtility.DisplayDialog("Chirp: Exporting Package", "Couldn't find any files to export", "Close");
				return;
			}

			var paths = new string[files.Length];
			for (var i = 0; i < files.Length; i++) paths[i] = AssetDatabase.GUIDToAssetPath(files[i]);
			AssetDatabase.ExportPackage(paths, $"Chirp_{Chirp.Version}.unitypackage", ExportPackageOptions.Recurse);
			FileUtil.MoveFileOrDirectory(to, from);
			AssetDatabase.Refresh();
		}

		
	}
}