using System;
using System.Collections.Generic;
using System.Linq;
//using Assets.Scripts.Gameplay.Monsters;
using Assets.Scripts.Static.Monsters;
using Gameplay.Components;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Gameplay
{
	public class WaveController : MonoBehaviour
	{
		private PlayfieldView _playfield;
		public ReactiveProperty<Wave> CurrentWave { get; } = new ReactiveProperty<Wave>();
		//public ReactiveCollection<MonsterView> Monsters { get; } = new ReactiveCollection<MonsterView>();
		//public int MonstersCount => Monsters.Count;
		public FloatReactiveProperty TimeLeftToWaveEnd { get; } = new FloatReactiveProperty();
		public IntReactiveProperty CurrentWaveIndex { get; } = new IntReactiveProperty();
		
		private List<Wave> _waves;
		private int _spawnedMonstersInCurrentWave;
		private float _timePastFromWaveStart;
		private float _timePastFromLastSpawn;

		//public event Action<MonsterView> OnMonsterDied;

		public void Init(PlayfieldView playfield)
		{
			_playfield = playfield;
			_waves = Game.Static.Waves.All.Values.ToList();
			CurrentWaveIndex.Value = -1;
			
			NextWave();
		}

		private void Update()
		{
			if (_playfield.Stats.IsFinished)
				return;
			
			_timePastFromWaveStart += Time.deltaTime;
			_timePastFromLastSpawn += Time.deltaTime;
			TimeLeftToWaveEnd.Value = Mathf.Max(0, CurrentWave.Value.NextDelay - _timePastFromWaveStart);
			
			if (_spawnedMonstersInCurrentWave < CurrentWave.Value.SpawnCount && _timePastFromLastSpawn >= CurrentWave.Value.SpawnInterval)
			{
				_timePastFromLastSpawn = 0;
				//SpawnMonster();
			}

			if (_timePastFromWaveStart >= CurrentWave.Value.NextDelay)
				NextWave();
		}

		public void NextWave()
		{
			CurrentWaveIndex.Value++;
			
			if (CurrentWaveIndex.Value < _waves.Count)
			{
				_spawnedMonstersInCurrentWave = 0;
				_timePastFromLastSpawn = 0;
				_timePastFromWaveStart = 0;
				TimeLeftToWaveEnd.Value = Mathf.Max(0, _waves[CurrentWaveIndex.Value].NextDelay);
				CurrentWave.Value = _waves[CurrentWaveIndex.Value];
			}
		}

		/**
		private void OnMonsterDead(MonsterView monster)
		{
			Monsters.Remove(monster);
			
			if (monster.IsBoss)
			{
				var bossesCount = Monsters.Count(m => m.IsBoss && !m.IsDead);
				if (bossesCount == 0 && CurrentWave.Value.Boss != null)
					NextWave();
			}
			
			OnMonsterDied?.Invoke(monster);
		}

		private void SpawnMonster()
		{
			_spawnedMonstersInCurrentWave++;

			var monsterData = CurrentWave.Value.Monster ?? CurrentWave.Value.Boss;
			var monsterId = CurrentWave.Value.Monster != null ? "11" : "boss";
			
			var monsterPlayer = _playfield.PoolManager.Monsters.Rent(monsterId);
			monsterPlayer.Init(monsterData, _playfield.Player.Path, CurrentWave.Value);
			monsterPlayer.OnDead.Then(() => OnMonsterDead(monsterPlayer));
			Monsters.Add(monsterPlayer);
			
			var monsterBot = _playfield.PoolManager.Monsters.Rent(monsterId);
			monsterBot.Init(monsterData, _playfield.Bot.Path, CurrentWave.Value);
			monsterBot.OnDead.Then(() => OnMonsterDead(monsterBot));
			Monsters.Add(monsterBot);
		}
		*/
	}
}