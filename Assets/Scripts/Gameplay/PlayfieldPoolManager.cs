using Assets.Scripts;
using Assets.Scripts.Animations;
using Assets.Scripts.Gameplay;
using Gameplay.Components;
using TMPro;
using UnityEngine;

namespace Gameplay
{
	public class PlayfieldPoolManager
	{
		public GameObjectPool<TMP_Text> DamageTexts;
		public PoolObjectList<ExplosionAnimation, ExplosionAnimation> Effects;
		private PlayfieldView _playfield;
		
		public PlayfieldPoolManager(PlayfieldView playfieldView)
		{
			_playfield = playfieldView;

			DamageTexts = new GameObjectPool<TMP_Text>(Game.BasePrefabs.LifeChangeText, _playfield.transform);
			Effects = new PoolObjectList<ExplosionAnimation, ExplosionAnimation>(_playfield.transform, prefab => prefab);
		}
	}
}