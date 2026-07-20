using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using WhiteSparrow.Shared.Logging;
using WhiteSparrow.Shared.Logging.Outputs;

namespace WhiteSparrow.Shared.LogTesting
{
	public class LogTester : MonoBehaviour
	{
		private float m_timeSinceLog = 0;

		public void Start()
		{
			Chirp.AddPlugin<UnityConsolePlugin>();
			Runner().Forget();
			
			Chirp.Logger.Error("TEST THIS");
		}
		//
		// public void Update()
		// {
		// 	m_timeSinceLog += Time.deltaTime;
		//
		// 	if (m_timeSinceLog < 1f)
		// 	{
		// 		return;
		// 	}
		// 	
		// 	m_timeSinceLog = 0;
		// }

		private async UniTask Runner()
		{
			LogLevel[] enumValues = Enum.GetValues(typeof(LogLevel)) as LogLevel[];
			while (true)
			{
				await UniTask.WaitForSeconds(1);

				var log = "LogTester".AsChirpLog();
				log.Level = enumValues[UnityEngine.Random.Range(0, enumValues.Length)];
				log.Context = this;
				Chirp.Impl.Submit(log);
			}
		}

		private void Update()
		{
			LogLevel[] enumValues = Enum.GetValues(typeof(LogLevel)) as LogLevel[];
			
			Chirp.Logger.Log("Test Log");
			Chirp.Logger.Warning("Test Warning"); 
			Chirp.Logger.Error("Test Error");
		}
	}
}