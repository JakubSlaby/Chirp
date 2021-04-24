using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WhiteSparrow.Shared.Logging.Initialize;

namespace WhiteSparrow.Shared.Logging
{
	[CustomEditor(typeof(ChirpInitialize))]
	public class ChirpInitializeInspector : Editor
	{
		private static class Styles
		{
			public static readonly GUIStyle InfoBox;
			public static readonly GUIStyle InfoMessage;
			public static readonly GUIStyle InfoButtonBar;
			
			public static readonly GUIStyle LoggerFrame;
			
			static Styles()
			{
				InfoBox = new GUIStyle("FrameBox");
				InfoBox.margin = new RectOffset(InfoBox.margin.left, InfoBox.margin.right, 6, 4);
				InfoBox.padding = new RectOffset(6, 6, 6, 6);
				
				
				InfoMessage = new GUIStyle(EditorStyles.label);
				InfoMessage.richText = true;
				InfoMessage.wordWrap = true;
				InfoMessage.stretchWidth = true;

				InfoButtonBar = new GUIStyle();
				InfoButtonBar.margin = new RectOffset(0, 0, 6, 2);
				
				LoggerFrame  = new GUIStyle("FrameBox");
			}
		}
		
		private TypeCache.TypeCollection m_ComponentTypeCollection;
		private List<Type> m_ComponentTypes;
		private void OnEnable()
		{
			m_ComponentTypeCollection = TypeCache.GetTypesDerivedFrom<IChirpLoggerInitializeComponent>();
			if (m_ComponentTypes == null)
				m_ComponentTypes = new List<Type>();
			else
				m_ComponentTypes.Clear();

			foreach (var type in m_ComponentTypeCollection)
			{
				if (type.IsAbstract || type.IsGenericType)
					continue;
				m_ComponentTypes.Add(type);
			}
		}

		protected ChirpInitialize TargetComponent => (ChirpInitialize) target;

		public override void OnInspectorGUI()
		{
			DrawBasicInfo();
			
			#if !CHIRP

			DrawChirpDisabled();
			
			#endif
			
			if (m_ComponentTypes == null || m_ComponentTypes.Count == 0)
			{
				DrawErrorNoLoggers();
				return;
			}

			DrawLoggerList();
		}


		private void DrawBasicInfo()
		{
			GUILayout.Label("Chirp Logging Framework: Initialize Helper", EditorStyles.boldLabel);
			using (new GUILayout.VerticalScope(Styles.InfoBox))
			{
				GUILayout.Label("This helper allows you to easily initialize different chirp Loggers (if present in project).", Styles.InfoMessage);

				using (new GUILayout.HorizontalScope(Styles.InfoButtonBar))
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Settings", EditorStyles.miniButton))
					{
						ChirpProjectSettingsInspector.ShowWindow();
					}

					if (GUILayout.Button("GitHub", EditorStyles.miniButton))
					{
						ChirpProjectSettingsInspector.VisitGitHub();
					}
				}
			}
			
		}
		
		private void DrawChirpDisabled()
		{
			Color bgColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.yellow;
			EditorGUILayout.HelpBox("Chirp Logging Framework is disabled on current platform. Enable it in Project Settings.", MessageType.Warning);
			GUI.backgroundColor = bgColor;
		}

		private void DrawErrorNoLoggers()
		{
			Color bgColor = GUI.backgroundColor;
			GUI.backgroundColor = Color.red;
			EditorGUILayout.HelpBox("No Logger initialize components found in the project.", MessageType.Error);
			GUI.backgroundColor = bgColor;
		}

		private Editor m_InlineEditor;
		private void DrawLoggerList()
		{
			GUILayout.Space(10);
			GUILayout.Label("Available Loggers:", EditorStyles.boldLabel);
			
			foreach (var type in m_ComponentTypes)
			{
				ExtractLoggerInfo(type, out Type loggerType, out bool showEditor);
				var component = TargetComponent.GetComponent(type);
				bool isEnabled = component != null;
				bool isToggled = false;

				Color bgColor = GUI.backgroundColor;
				GUI.backgroundColor = isEnabled ? bgColor : Color.red;
				using (new GUILayout.VerticalScope(Styles.LoggerFrame))
				{
					GUI.backgroundColor = bgColor;
					isToggled = EditorGUILayout.ToggleLeft(loggerType.Name, isEnabled);
				}
				
				if (isEnabled != isToggled)
				{
					ToggleComponent(type, isToggled);
					continue;
				}
				
				if(isEnabled && showEditor)
				{
					if(m_InlineEditor == null || m_InlineEditor.target != component)
						m_InlineEditor = Editor.CreateEditor(component);
					using (new GUILayout.VerticalScope(Styles.LoggerFrame))
					{
						m_InlineEditor.OnInspectorGUI();
					}
				}
			}
		}

		private void ToggleComponent(Type type, bool toggle)
		{
			Undo.IncrementCurrentGroup();
			
			var component = TargetComponent.GetComponent(type);
			if (component == null && toggle)
			{
				component = Undo.AddComponent(TargetComponent.gameObject, type);
				component.hideFlags = HideFlags.HideInInspector;
			}
			else if (component != null && !toggle)
			{
				Undo.DestroyObjectImmediate(component);
			}

			var allComponents = TargetComponent.GetComponents<IChirpLoggerInitializeComponent>();
			List<Component> outputList = new List<Component>();
			foreach (var c in allComponents)
			{
				if(c is Component cC)
					outputList.Add(cC);
			}
			
			TargetComponent.m_InitializeComponents = outputList;
			Undo.RecordObject(TargetComponent.gameObject, "Chirp: Toggle Initialized Logger");
		}

		
		private Type m_HelperInitializeBaseType = typeof(AbstractLoggerInitializeComponent<>);
		private void ExtractLoggerInfo(Type initializeType, out Type loggerType, out bool showEditor)
		{
			showEditor = initializeType.GetCustomAttribute<ShowInitializeOptionsAttribute>() != null;
			Type t = initializeType.BaseType;
			while (t != null)
			{
				if (t.IsGenericType && t.GetGenericTypeDefinition() == m_HelperInitializeBaseType)
				{
					var genericArguments = t.GetGenericArguments();
					if (genericArguments.Length > 0)
					{
						
						loggerType = genericArguments[0];
						return;
					}
				}
				t = t.BaseType;
			}

			loggerType = null;
		}
	}
}