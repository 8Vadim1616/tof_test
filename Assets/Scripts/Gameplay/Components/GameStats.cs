using System.Collections.Generic;
using Assets.Scripts.Gameplay;
using Assets.Scripts.User;
using Newtonsoft.Json;
using UniRx;

namespace Gameplay.Components
{
	public enum GameResult
	{
		NONE = 0,
		WIN = 1,
		LOSE = 2
	}
	
	[JsonObject(MemberSerialization.OptIn)]
	public class GameStats
	{
		private PlayfieldView _playfield;
		
		[JsonProperty("rs")]
		public GameResult Result { get; private set; }
		
		[JsonProperty("p1_dmg")]
		public float PlayerDamage { get; private set; }
		
		[JsonProperty("p2_dmg")]
		public float CoopDamage { get; private set; }
		
		[JsonProperty("wave")]
		public int Wave { get; private set; }
		
		[JsonProperty("floors")]
		public Dictionary<int, int> Floors { get; private set; }

		[JsonProperty("items")]
		public Dictionary<int, int> Items { get; private set; }
		
		public int Level  { get; set; }
		
		public UserData Coop { get; private set; }
		
		

		public bool IsWin => false;
		public bool IsFinished => Result != GameResult.NONE;
		
		public GameStats(PlayfieldView playfieldView)
		{
			Floors = new Dictionary<int, int>();
			Items = new Dictionary<int, int>();
			_playfield = playfieldView;
			Level = 1;
			//Coop = _playfield.Bot.User;
			/**
			_playfield.WaveController.CurrentWave.Subscribe(_ =>
			{
				Wave = _.Id;
			}).AddTo(_playfield.gameObject);
			*/
		}

		public void AddFloorStats(int level, int attempts)
		{
			Floors.Add(level, attempts);
		}
		
		public void FinishGame(GameResult result)
		{
			Result = result;
		}

		/**
		public void OnMonsterAttack(UnitView unit, float damage)
		{
			if (unit && unit.Group && unit.Group.PlayerComponent.IsCurrentPlayer)
				PlayerDamage += damage;
			else
				CoopDamage += damage;
		}
		*/

		public int PlayerPrc =>
			PlayerDamage + CoopDamage > 0 ? (int) (PlayerDamage / (PlayerDamage + CoopDamage) * 100f) : 0;
	}
}