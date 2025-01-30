#if UNITY_EDITOR
using Assets.Scripts;
using Assets.Scripts.Gameplay;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.UI.Windows.Units;
using UnityEngine;

public class DebugTool : MonoBehaviour
{
	private double i = 1;
	
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			Game.Instance.Playfiled.Value.WaveController.NextWave();
		}
		
		if (Input.GetKeyDown(KeyCode.F2))
		{
			PlayfieldView.EndGame();
		}
		
		if (Input.GetKeyDown(KeyCode.F3))
		{
			Game.Instance.Playfiled.Value.Player.WaveCoin.Value += 200;
			//Game.Instance.Playfiled.Value.Bot.WaveCoin.Value += 200;
		}
	}
}
#endif