using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	[CustomEditor(typeof(ChirpProjectSettings))]
	public class ChirpProjectSettingsInspector : Editor
	{
		private static class Styles
		{
			public static readonly GUIStyle WelcomeMessage;
			public static readonly GUIStyle WelcomeMessageLabel;
			public static readonly GUIStyle VersionLabel;
			public static readonly GUIStyle WelcomeMessageButton;
			public static readonly GUIStyle EditorCompilingMessageContainer;

			public static readonly GUIStyle Header;
			public static readonly GUIStyle PlatformTableToggleButton;
			public static readonly GUIStyle PlatformTableRow;
			public static readonly GUIStyle PlatformTableLabel;
			public static readonly GUIStyle PlatformTableLabelCurrent;
			
			public static readonly GUIStyle PlatformTableInfoColumn;
			public static readonly GUIStyle PlatformTableLogLevelColumn;
			
			static Styles()
			{
				WelcomeMessage = new GUIStyle("FrameBox");
				WelcomeMessage.padding = new RectOffset(10, 10, 12, 12);
				
				WelcomeMessageLabel = new GUIStyle(EditorStyles.label);
				WelcomeMessageLabel.richText = true;
				WelcomeMessageLabel.wordWrap = true;
				WelcomeMessageLabel.stretchWidth = true;
				WelcomeMessageButton = new GUIStyle(EditorStyles.miniButton);

				VersionLabel = new GUIStyle(EditorStyles.miniLabel);
				VersionLabel.margin = new RectOffset(0, 0, 10, 0);
				
				EditorCompilingMessageContainer = new GUIStyle("FrameBox");
				EditorCompilingMessageContainer.padding = new RectOffset(10, 10, 12, 12);

				Header = new GUIStyle(EditorStyles.boldLabel);
				Header.fontSize = 14;
				Header.margin = new RectOffset(0, 0, 20, 14);
				
				PlatformTableRow = new GUIStyle("FrameBox");
				PlatformTableInfoColumn = new GUIStyle();
				PlatformTableLogLevelColumn = new GUIStyle();

				PlatformTableLabelCurrent = new GUIStyle("AssetLabel");
				PlatformTableLabelCurrent.imagePosition = ImagePosition.ImageLeft;
				PlatformTableLabelCurrent.fixedHeight = 24f;
				PlatformTableLabelCurrent.padding = new RectOffset(10, 16, 4, 0);
				
				
				PlatformTableLabel = new GUIStyle(PlatformTableLabelCurrent);
				PlatformTableLabel.normal.background = null;

				PlatformTableToggleButton = new GUIStyle();
				PlatformTableToggleButton.stretchWidth = false;
				PlatformTableToggleButton.padding = new RectOffset(0, 0, 3, 3);
			}
		}

		private BuildTargetGroup[] m_AvailableBuildTargetGroups;
		private Dictionary<BuildTargetGroup, bool> m_BuildTargetGroupToChirpEnabled;
		private Dictionary<BuildTargetGroup, int> m_BuildTargetGroupToChirpLevel;
		private Dictionary<BuildTargetGroup, GUIContent> m_BuildTargetGroupToLabel;

		private GUIContent m_ContentPlatformSettingsHelpMessage;

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
				string scriptingDefines = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup));
				m_BuildTargetGroupToChirpEnabled.Add(buildTargetGroup, scriptingDefines.Contains("CHIRP"));
				m_BuildTargetGroupToChirpLevel.Add(buildTargetGroup, ChirpBuildUtils.GetLogLevelFromScriptDefines(scriptingDefines));
				string groupName = buildTargetGroup.ToString();
				m_BuildTargetGroupToLabel.Add(buildTargetGroup, EditorGUIUtility.TrTextContentWithIcon(groupName, $"d_BuildSettings.{groupName}.Small"));
			}
		}

		public override void OnInspectorGUI()
		{
			GUILayout.Space(20);

			using (new GUILayout.HorizontalScope(Styles.WelcomeMessage))
			{
				using (new GUILayout.VerticalScope())
				{
					GUILayout.Label("Thanks for using Chirp!\nIf you find any issues or have ideas for enhancements, please post them in GitHub Issues!", Styles.WelcomeMessageLabel);
					GUILayout.Label($"Version {Chirp.Version}", Styles.VersionLabel);
				}
				if (GUILayout.Button("GitHub", Styles.WelcomeMessageButton, GUILayout.MaxWidth(100)))
				{
					VisitGitHub();
				}
			}

			if (EditorApplication.isCompiling)
			{
				using (new GUILayout.HorizontalScope(Styles.EditorCompilingMessageContainer))
				{
					var messageContent = EditorGUIUtility.TrTextContentWithIcon("Editor is compiling, please wait...", "Loading");
					GUILayout.FlexibleSpace();
					GUILayout.Label(messageContent);
					GUILayout.FlexibleSpace();
				}

				return;
			}

			PlatformSettingsGUI();
		}

		private void PlatformSettingsGUI()
		{
			var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
			var activeBuildTargetGroup = BuildPipeline.GetBuildTargetGroup(activeBuildTarget);
			var enabledIconContent = EditorGUIUtility.IconContent("d_GreenCheckmark");
			var disabledIconContent = EditorGUIUtility.IconContent("d_winbtn_mac_close_a");
			
			GUILayout.Label("Platform Compilation Settings", Styles.Header);
			EditorGUILayout.HelpBox("Configure logging for each target platform. You can completely disable logging on selected platforms and define which logging level will be active.", MessageType.Info);
			
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
					using (new GUILayout.HorizontalScope(Styles.PlatformTableInfoColumn, GUILayout.Width(200), GUILayout.ExpandWidth(true)))
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
				}
			}
		}
		
		
		[SettingsProvider]
		static SettingsProvider CreateChirpSettingsProvider()
		{
			var provider = AssetSettingsProvider.CreateProviderFromObject("Project/White Sparrow/Chirp Logging Framework", ChirpProjectSettings.Instance);
			return provider;
		}
		 
		[MenuItem("Tools/White Sparrow/Chirp Logger/Chirp Settings", priority = 290)]
		internal static void ShowWindow()
		{
			SettingsService.OpenProjectSettings("Project/White Sparrow/Chirp Logging Framework");
		}

		internal static void VisitGitHub()
		{
			Application.OpenURL("https://github.com/JakubSlaby/Chirp");
		}
	}
}