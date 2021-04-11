using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	[HelpURL("https://github.com/JakubSlaby/Chirp")]
	public class ChirpProjectSettings : ScriptableObject
	{
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
		
		private static ChirpProjectSettings LoadSettings()
		{
			// Fetch a settings objects loaded before a domain reload
			ChirpProjectSettings[] settingsInstances = Resources.FindObjectsOfTypeAll<ChirpProjectSettings>();
			
			if (settingsInstances.Length > 1)
			{
				// if for any reason we have more than one instance, destroy the excess
				for (int i = settingsInstances.Length - 1; i>0; i--)
				{
					ScriptableObject.DestroyImmediate(settingsInstances[i], true);
				}

				return settingsInstances[0];
			}
			if (settingsInstances.Length == 1)
			{
				return settingsInstances[0];
			}

			return ScriptableObject.CreateInstance<ChirpProjectSettings>();
		}

#endregion
		
	}
}