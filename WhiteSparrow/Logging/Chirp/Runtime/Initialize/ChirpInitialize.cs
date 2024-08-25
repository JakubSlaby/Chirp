using System.Collections.Generic;
using UnityEngine;

namespace WhiteSparrow.Shared.Logging.Initialize
{
	[HelpURL("https://github.com/JakubSlaby/Chirp")]
	[AddComponentMenu("White Sparrow/Chirp/Chirp Initialize")]
	public class ChirpInitialize : MonoBehaviour
	{
		public List<Component> m_InitializeComponents;

#if CHIRP
		private void Awake()
		{
			if (m_InitializeComponents == null || m_InitializeComponents.Count == 0)
				return;

			foreach (var component in m_InitializeComponents)
			{
				if(component is IChirpLoggerInitializeComponent initializeComponent)
					initializeComponent.Initialize();
			}
		}

#endif

		
	}
}