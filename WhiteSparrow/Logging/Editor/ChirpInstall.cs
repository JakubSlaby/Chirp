using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging
{
	public class ChirpInstall : EditorWindow
	{
		private static class Styles
		{
			public static readonly GUIStyle PrimaryContainer;
			public static readonly GUIStyle TitleLabel;
			public static readonly GUIStyle MultilineLabel;
			public static readonly GUIStyle InstallButton;

			static Styles()
			{
				PrimaryContainer = new GUIStyle();
				PrimaryContainer.padding = new RectOffset(6, 6, 4, 4);
				TitleLabel = new GUIStyle(EditorStyles.boldLabel);
				TitleLabel.alignment = TextAnchor.MiddleCenter;
				TitleLabel.fontSize *= 2;
				TitleLabel.margin = new RectOffset(TitleLabel.margin.left, TitleLabel.margin.right, 14, 14);
				MultilineLabel = new GUIStyle(EditorStyles.textArea);
				MultilineLabel.richText = true;
				MultilineLabel.margin = new RectOffset(MultilineLabel.margin.left, MultilineLabel.margin.right, 8, 8);
				MultilineLabel.padding = new RectOffset(8, 8, 8, 8);
				InstallButton = new GUIStyle("button");
				InstallButton.fontSize = Mathf.CeilToInt(InstallButton.fontSize * 1.3f);
				InstallButton.padding = new RectOffset(10, 10, 6, 6);
			}
		}
		
		private void OnEnable()
		{
			this.titleContent = EditorGUIUtility.TrTextContent("Chirp");
		}

		private void OnGUI()
		{
			bool performInstall = false;
			
			using (new GUILayout.VerticalScope(Styles.PrimaryContainer))
			{
				GUILayout.Label("Chirp Logging Framework", Styles.TitleLabel);
				GUILayout.Label("Welcome, and thanks for downloading Chirp Logging Framework.\n\nThe installation process will configure Chirp script defines on available platforms and create a settings asset in the ProjectSettings directory.\n\nYou can access Chirp settings in Project Settings/White Sparrow/Chirp.", Styles.MultilineLabel);

				if (ChirpProjectSettings.Instance == null || ChirpProjectSettings.Instance.m_RequiresInstalation)
				{
					performInstall = ShowInstallButton();
				}
				else
				{
					ShowInstalledInfo();
				}
			}
			
			if (performInstall)
			{
				PerformInstall();
			}
		}

		private bool ShowInstallButton()
		{
			bool performInstall = false;
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				performInstall = GUILayout.Button("Install", Styles.InstallButton, GUILayout.MaxWidth(300));
				GUILayout.FlexibleSpace();			
			}

			return performInstall;
		}

		private void ShowInstalledInfo()
		{
			using (new GUILayout.HorizontalScope())
			{
				GUILayout.FlexibleSpace();
				if (GUILayout.Button("Completed - Close", Styles.InstallButton, GUILayout.MaxWidth(300)))
				{
					this.Close();
				}
				GUILayout.FlexibleSpace();			
			}
		}

		private void PerformInstall()
		{
			var settings = ChirpProjectSettings.Instance;
			settings.Version = Chirp.Version;
			settings.m_RequiresInstalation = false;
			ChirpProjectSettings.WriteSettings(settings);

			var availableBuildTargetGroups = ChirpBuildUtils.GetAvailableBuildTargetGroups();
			foreach (var targetGroup in availableBuildTargetGroups)
			{
				ChirpBuildUtils.SetTargetGroupChirpEnabled(targetGroup, true);
				ChirpBuildUtils.SetTargetGroupLogLevel(targetGroup, 0);
			}
		}

		[DidReloadScripts]
		private static void InstallHook()
		{
			EditorApplication.update += Install;
		}
		private static void Install()
		{
			EditorApplication.update -= Install;
			var chirpSettings = ChirpProjectSettings.Instance;
			if (chirpSettings != null && !chirpSettings.m_RequiresInstalation)
				return;

			ChirpInstall window = EditorWindow.GetWindow<ChirpInstall>();
			window.Show();
		}
	}
}