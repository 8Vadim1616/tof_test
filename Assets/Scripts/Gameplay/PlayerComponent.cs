using System.Linq;
using Assets.Scripts.Static.Items;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Gameplay;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
	public class PlayerComponent : MonoBehaviour
	{

		public FloatReactiveProperty WaveCoin { get; private set; } = new();
		public FloatReactiveProperty WaveSpecialCoin { get; private set; } = new();
		public LongReactiveProperty UnitsCount { get; } = new ();
		//public PlayerUnitUpgrades UnitUpgrades { get; private set; }

		public PlayfieldView Playfield { get; private set; }
		public UserData User { get; private set; }
		public bool Inited { get; private set; }
		public bool IsCurrentPlayer => Playfield && Playfield.Player == this;

		public void Init(UserData user)
		{
			User = user;
			Playfield = GetComponentInParent<PlayfieldView>();
			//Playfield.WaveController.OnMonsterDied += OnMonsterDead;

			WaveCoin.Value = Game.Settings.WaveCoinStartCount;
			WaveSpecialCoin.Value = Game.Settings.WaveSpecialCoinStartCount;
			
			//Даём начальные монеты за старт волны
			/**
			Playfield.WaveController.CurrentWave.Subscribe(newWave =>
			{
				WaveCoin.Value += Playfield.AbilitiesController.CoinsHeldPerWavePrc.Value * WaveCoin.Value;
				
				if (newWave != null)
					WaveCoin.Value += newWave.StartGivePoint;
				
				WaveCoin.Value += Playfield.AbilitiesController.WaveCoinsForWave.Value;

				if ((newWave.Id - 1) % 10 == 0)
					WaveSpecialCoin.Value += Playfield.AbilitiesController.WaveSpecialCoinsEvery10Waves.Value;
			}).AddTo(Playfield.gameObject);

			UnitUpgrades = new PlayerUnitUpgrades(this);
			*/
			Inited = true;
		}

		public FloatReactiveProperty GetCoinsReactive(Item item)
			=> item == Game.Static.Items.WaveCoin ? WaveCoin : WaveSpecialCoin;

		private void OnDestroy()
		{
			/**
			if (Playfield && Playfield.WaveController)
				Playfield.WaveController.OnMonsterDied -= OnMonsterDead;
				*/
		}

		/**
		private void OnMonsterDead(MonsterView monster)
		{
			if (monster.Wave.DropOnDeath.IsNullOrEmpty())
				return;
			
			foreach (var drop in monster.Wave.DropOnDeath)
			{
				if (drop.Item == Game.Static.Items.WaveCoin)
					WaveCoin.Value += drop.Count + Playfield.AbilitiesController.WaveCoinsWhenKillAdd.Value;
				else if (drop.Item == Game.Static.Items.WaveSpecialCoin)
					WaveSpecialCoin.Value += drop.Count;
			}
		}

		public void UpdateUnitsCount()
		{
			UnitsCount.Value = _positions.All.Sum(p => p.UnitGroup ? p.UnitGroup.Units.Count : 0);
		}
		*/
	}
}