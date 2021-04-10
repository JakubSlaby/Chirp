using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace WhiteSparrow.Shared.Logging
{
	public class LogChannelCompiler
	{
		private const string DialogTitle = "Chirp: LogChannels Generator";

		private const string ListTemplate = @"
namespace ${NAMESPACE}
{
	public class ${CLASS_NAME} : WhiteSparrow.Shared.Logging.AbstractLogChannelList
	{
		[UnityEngine.RuntimeInitializeOnLoadMethod]
		private static void RuntimeInitializeOnLoad()
		{
			WhiteSparrow.Shared.Logging.LogChannel.RegisterChannelTarget(s_ChannelList);
		}

		private static readonly System.Type[] s_ChannelList = new System.Type[]
		{
	${TYPE_LIST}
		};
		
		public override System.Type[] GetChannelList()
		{
			return s_ChannelList;
		}
	}
}
";


		[MenuItem("Tools/Chirp Logger/Generate Log Channels List", priority = 300)]
		public static void GenerateLogChannelsList()
		{
			var typesForGeneration = TypeCache.GetTypesWithAttribute<LogChannelAttribute>();
			var existingItems = FindExistingLogChannelsList(out var indexedChannels);

			if (typesForGeneration.Count == 0 && existingItems.Length == 0)
			{
				EditorUtility.DisplayDialog(DialogTitle, "No Types marked as LogChannels found", "Ok");
				return;
			}

			FileInfo outputFileInfo = null;
			Type outputFileType = null;
			if (existingItems.Length == 1)
			{
				if (TypeListCompare(typesForGeneration, indexedChannels))
				{
					EditorUtility.DisplayDialog(DialogTitle,
						$"The generated list is correct. No changes needed.\npath: {existingItems[0].Item2}", "Ok");
					return;
				}

				outputFileType = existingItems[0].Item1;
				outputFileInfo = new FileInfo(existingItems[0].Item2);
			}

			foreach (var existingItem in existingItems) AssetDatabase.DeleteAsset(existingItem.Item2);

			if (typesForGeneration.Count == 0)
			{
				EditorUtility.DisplayDialog(DialogTitle, "No Types marked as LogChannels found", "Ok");
				return;
			}

			if (outputFileInfo == null)
			{
				var selectedPath = EditorUtility.SaveFilePanelInProject("Chirp: Select LogChannel list location.",
					"LogChannelList", "cs", "Select location for the generated LogChannels list.");
				if (string.IsNullOrWhiteSpace(selectedPath))
					return;

				outputFileInfo = new FileInfo(selectedPath);
			}

			GenerateFile(outputFileType, outputFileInfo, typesForGeneration);
		}


		private static Tuple<Type, string>[] FindExistingLogChannelsList(out HashSet<Type> indexedTypes)
		{
			indexedTypes = new HashSet<Type>();
			var existingListTypes = TypeCache.GetTypesDerivedFrom<AbstractLogChannelList>();
			if (existingListTypes.Count == 0)
				return Array.Empty<Tuple<Type, string>>();

			var output = new List<Tuple<Type, string>>();
			foreach (var type in existingListTypes)
			{
				var scriptableObject = ScriptableObject.CreateInstance(type);
				scriptableObject.hideFlags = HideFlags.HideAndDontSave;
				var monoScript = MonoScript.FromScriptableObject(scriptableObject);
				var path = AssetDatabase.GetAssetPath(monoScript);
				if (string.IsNullOrEmpty(path))
				{
					Debug.LogError(
						$"Chirp: LogChannel List generation found type {type.FullName} for replacement when generating, but couldn't find the file path. It's possible that the file name is not the same as the Type name.");
					continue;
				}

				if (scriptableObject is AbstractLogChannelList logChannelList)
				{
					var indexedChannels = logChannelList.GetChannelList();
					foreach (var channel in indexedChannels) indexedTypes.Add(channel);
				}

				Object.DestroyImmediate(scriptableObject, true);

				output.Add(Tuple.Create(type, path));
			}

			return output.ToArray();
		}


		private static void GenerateFile(Type fileType, FileInfo fileInfo, TypeCache.TypeCollection typeList)
		{
			var typeListBuilder = new StringBuilder();
			foreach (var type in typeList) typeListBuilder.AppendLine($"			typeof({type.FullName}),");

			var content = ListTemplate.Replace("${TYPE_LIST}", typeListBuilder.ToString())
									  .Replace("${CLASS_NAME}", fileInfo.Name.Replace(fileInfo.Extension, ""))
									  .Replace("${NAMESPACE}", fileType != null ? fileType.Namespace : "WhiteSparrow.Shared.Logging");
			File.WriteAllText(fileInfo.FullName, content);

			AssetDatabase.Refresh();
		}

		private static bool TypeListCompare(TypeCache.TypeCollection typeCandidates, HashSet<Type> typesRegistered)
		{
			if (typeCandidates.Count != typesRegistered.Count)
				return false;

			foreach (var type in typeCandidates)
				if (!typesRegistered.Contains(type))
					return false;

			return true;
		}
	}
}