using System.IO;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
    public static class ExportPackage
    {
        private const string s_ContentDirectory = "WhiteSparrow/";
        private const string s_ExportDirectory = "Plugins/WhiteSparrow/";
        
        [MenuItem("Tools/Chirp Logger/Export Package", priority = 600)]
        public static void ExportChirpPackage()
        {
            DirectoryInfo rootDirectory = new DirectoryInfo(ChirpDevelopment.ChirpRepositoryRoot);
            DirectoryInfo contentDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, s_ContentDirectory));
            DirectoryInfo targetDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, s_ExportDirectory));
            
            Debug.Log($"root: {rootDirectory.FullName}\ncontent: {contentDirectory.FullName}\ntarget: {targetDirectory.FullName}");

            string from = ChirpDevelopment.MakePathRelative(contentDirectory.FullName);
            string to = ChirpDevelopment.MakePathRelative(targetDirectory.FullName);
            Debug.Log($"from: {from} to: {to}");
            FileUtil.MoveFileOrDirectory(from, to);
            AssetDatabase.Refresh();

            var files = AssetDatabase.FindAssets("*", new string[] {to.TrimEnd('/')});
            if (files.Length == 0)
            {
                EditorUtility.DisplayDialog("Chirp: Exporting Package", "Couldn't find any files to export", "Close");
                return;
            }
            var paths = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                paths[i] = AssetDatabase.GUIDToAssetPath(files[i]);
            }
            AssetDatabase.ExportPackage(paths, $"Chirp_{Chirp.Version}.unitypackage", ExportPackageOptions.Recurse);
            

            FileUtil.MoveFileOrDirectory(to, from);
            AssetDatabase.Refresh();
        }

    
    }
}