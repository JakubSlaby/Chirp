using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	[CustomEditor(typeof(ChirpProjectSettings))]
	public class ChirpProjectSettingsInspector : Editor
	{
		private static class Styles
		{
			public static readonly GUIStyle PlatformTableToggleButton;
			public static readonly GUIStyle PlatformTableRow;
			public static readonly GUIStyle PlatformTableLabel;
			public static readonly GUIStyle PlatformTableLabelCurrent;
			
			public static readonly GUIStyle PlatformTableInfoColumn;
			public static readonly GUIStyle PlatformTableLogLevelColumn;
			
			static Styles()
			{
				PlatformTableRow = new GUIStyle("FrameBox");
				PlatformTableInfoColumn = new GUIStyle();
				PlatformTableLogLevelColumn = new GUIStyle();

				PlatformTableLabelCurrent = new GUIStyle("AssetLabel");
				PlatformTableLabelCurrent.imagePosition = ImagePosition.ImageLeft;
				PlatformTableLabelCurrent.fixedHeight = 24f;
				PlatformTableLabelCurrent.padding = new RectOffset(10, 14, 4, 0);
				
				
				PlatformTableLabel = new GUIStyle(PlatformTableLabelCurrent);
				PlatformTableLabel.normal.background = null;

				PlatformTableToggleButton = new GUIStyle();
				PlatformTableToggleButton.stretchWidth = false;
				PlatformTableToggleButton.padding = new RectOffset(0, 0, 3, 3);
			}
			
		}
		
		private ChirpProjectSettings settings => (ChirpProjectSettings) target;

		private BuildTargetGroup[] m_AvailableBuildTargetGroups;
		private Dictionary<BuildTargetGroup, bool> m_BuildTargetGroupToChirpEnabled;
		private Dictionary<BuildTargetGroup, int> m_BuildTargetGroupToChirpLevel;
		private Dictionary<BuildTargetGroup, GUIContent> m_BuildTargetGroupToLabel;
		
		private void OnEnable()
		{
			m_AvailableBuildTargetGroups = ChirpBuildUtils.GetAvailableBuildTargetGroups();
			m_BuildTargetGroupToChirpEnabled = new Dictionary<BuildTargetGroup, bool>();
			m_BuildTargetGroupToChirpLevel = new Dictionary<BuildTargetGroup, int>();
			m_BuildTargetGroupToLabel = new Dictionary<BuildTargetGroup, GUIContent>();
			RefreshGroupData();
		}

		private void RefreshGroupData()
		{
			m_BuildTargetGroupToChirpEnabled.Clear();
			m_BuildTargetGroupToChirpLevel.Clear();
			m_BuildTargetGroupToLabel.Clear();
			foreach (var buildTargetGroup in m_AvailableBuildTargetGroups)
			{
				string scriptingDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
				m_BuildTargetGroupToChirpEnabled.Add(buildTargetGroup, scriptingDefines.Contains("CHIRP"));
				m_BuildTargetGroupToChirpLevel.Add(buildTargetGroup, ChirpBuildUtils.GetLogLevelFromScriptDefines(scriptingDefines));
				string groupName = buildTargetGroup.ToString();
				m_BuildTargetGroupToLabel.Add(buildTargetGroup, EditorGUIUtility.TrTextContentWithIcon(groupName, $"d_BuildSettings.{groupName}.Small"));
			}
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.LabelField("Current Version", settings.Version);

			GUILayout.Space(20);

			if (EditorApplication.isCompiling)
			{
				var messageContent = EditorGUIUtility.TrTextContentWithIcon("Editor is compiling, please wait...", "Loading");
				GUILayout.Label(messageContent);
				return;
			}

			PlatformSettingsGUI();
		}

		private void PlatformSettingsGUI()
		{
			var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);

			var enabledIconContent = EditorGUIUtility.IconContent("d_winbtn_mac_max");
			var disabledIconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close");
			
			GUILayout.Label("Platform Compilation Settings");
			foreach (var availableTargetGroup in m_AvailableBuildTargetGroups)
			{
				GUIStyle labelStyle = availableTargetGroup == activeBuildTargetGroup ? Styles.PlatformTableLabelCurrent : Styles.PlatformTableLabel;
				m_BuildTargetGroupToChirpEnabled.TryGetValue(availableTargetGroup, out var chirpEnabled);
				Color bgColor = GUI.backgroundColor;
				if(!chirpEnabled)
					GUI.backgroundColor = Color.red;
				using (new GUILayout.HorizontalScope(Styles.PlatformTableRow))
				{
					
					GUI.backgroundColor = bgColor;
					using (new GUILayout.HorizontalScope(Styles.PlatformTableInfoColumn, GUILayout.Width(200)))
					{
						GUILayout.Button(chirpEnabled ? enabledIconContent : disabledIconContent, Styles.PlatformTableToggleButton);

						if (m_BuildTargetGroupToLabel.TryGetValue(availableTargetGroup, out var labelContent))
						{
							GUILayout.Label(labelContent, labelStyle);
						}
						else
						{
							GUILayout.Label(availableTargetGroup.ToString(), labelStyle);
						}

						GUILayout.FlexibleSpace();

						if (chirpEnabled)
						{
							GUI.backgroundColor = Color.red;
							if (GUILayout.Button("Disable"))
							{
								ChirpBuildUtils.SetTargetGroupChirpEnabled(availableTargetGroup, false);
								RefreshGroupData();
								Repaint();
							}
						}
						else
						{
							GUI.backgroundColor = Color.green;
							if (GUILayout.Button("Enable"))
							{
								ChirpBuildUtils.SetTargetGroupChirpEnabled(availableTargetGroup, true);
								RefreshGroupData();
								Repaint();
							}
						}

						GUI.backgroundColor = bgColor;
					}

					using (new GUILayout.HorizontalScope(Styles.PlatformTableLogLevelColumn))
					{
						m_BuildTargetGroupToChirpLevel.TryGetValue(availableTargetGroup, out var levelInt);
						LogLevel currentLogLevel = (LogLevel) levelInt;
						LogLevel selectedLogLevel = (LogLevel)EditorGUILayout.EnumPopup(currentLogLevel);
						if (currentLogLevel != selectedLogLevel)
						{
							ChirpBuildUtils.SetTargetGroupLogLevel(availableTargetGroup, (int)selectedLogLevel);
							RefreshGroupData();
							Repaint();
						}
					}

					// string groupName = availableTargetGroup.ToString();
					// GUILayout.Label(EditorGUIUtility.TrTextContentWithIcon(groupName, $"d_BuildSettings.{groupName}.Small"), Styles.PlatformTableLabelColumn);
					// GUILayout.Button("enabled");
				}
			}
		}
		
		
		[SettingsProvider]
		static SettingsProvider CreateChirpSettingsProvider()
		{
			var provider = AssetSettingsProvider.CreateProviderFromObject("Project/White Sparrow/Chirp Logging Framework", ChirpProjectSettings.Instance);
			return provider;
		}
		 
		[MenuItem("Tools/Chirp Logger/Chirp", priority = 290)]
		private static void ShowWindow()
		{
			SettingsService.OpenProjectSettings("Project/White Sparrow/Chirp Logging Framework");
		}
	}
}