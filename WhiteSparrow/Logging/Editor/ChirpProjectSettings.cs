using System;
using System.IO;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	[HelpURL("https://github.com/JakubSlaby/Chirp")]
	public class ChirpProjectSettings : ScriptableObject
	{
		[SerializeField][HideInInspector]
		private string m_Version;

		public string Version
		{
			get => m_Version;
			internal set
			{
				m_Version = value;
			}
		}

		
		[SerializeField][HideInInspector]
		internal bool m_RequiresInstalation = false;

		

#region Instance Handling

		

		private static ChirpProjectSettings s_Instance;
		public static ChirpProjectSettings Instance
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = LoadSettings();
				}

				return s_Instance;
			}
		}

		internal static void WriteSettings(ChirpProjectSettings instance)
		{
			if (Instance != instance)
			{
				if(Application.isPlaying)
					ScriptableObject.Destroy(s_Instance);
				else
					ScriptableObject.DestroyImmediate(s_Instance);
				s_Instance = instance;
			}
			
			FileInfo chirpSettingsFile = new FileInfo(Application.dataPath + "/" + c_SettingsPath);
			if (chirpSettingsFile.Directory?.Exists == false)
			{
				Debug.LogError($"Unable to save Chirp Project Settings. The ProjectSettings path cannot be found. {chirpSettingsFile.Directory.FullName}");
				return;
			}
			
			string jsonOutput = JsonUtility.ToJson(instance);
			try
			{
				File.WriteAllText(chirpSettingsFile.FullName, jsonOutput);
			}
#pragma warning disable 168
			catch (Exception e)
#pragma warning restore 168
			{
				Debug.Log($"Was unable to write Chirp settings to file {chirpSettingsFile.FullName}.\nException: {e.Message}");
			}
		}

		private const string c_SettingsPath = "../ProjectSettings/Chirp.asset"; 
		private static ChirpProjectSettings LoadSettings()
		{
			// Fetch a settings objects loaded before a domain reload
			ChirpProjectSettings[] settingsInstances = Resources.FindObjectsOfTypeAll<ChirpProjectSettings>();
			if (settingsInstances.Length > 1)
			{
				for (int i = 0; i < settingsInstances.Length; i++)
				{
					ScriptableObject.DestroyImmediate(settingsInstances[i]);
				}
			}
			else if (settingsInstances.Length == 1)
			{
				return settingsInstances[0];
			}

			// Try loading from Project Settings
			ChirpProjectSettings chirp = ScriptableObject.CreateInstance<ChirpProjectSettings>();
			FileInfo chirpSettingsFile = new FileInfo(Application.dataPath + "/" + c_SettingsPath);
			if (chirpSettingsFile.Directory?.Exists == false)
			{
				chirp.m_RequiresInstalation = true;
			}
			
			if (chirpSettingsFile.Exists)
			{
				string jsonContent = File.ReadAllText(chirpSettingsFile.FullName);
				try
				{
					JsonUtility.FromJsonOverwrite(jsonContent, chirp);
				}
#pragma warning disable 168
				catch (Exception e)
#pragma warning restore 168
				{
					chirp = ScriptableObject.CreateInstance<ChirpProjectSettings>();
					chirp.m_RequiresInstalation = true;
				}
			}
			else
			{
				chirp.m_RequiresInstalation = true;
			}

			return chirp;
		}

#endregion
		
	}
}