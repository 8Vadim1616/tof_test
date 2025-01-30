using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.HUD;
using Assets.Scripts.UI.HUD.Panels;
using Assets.Scripts.Utils;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Scripts.UI
{
	public class DropController
	{
		public const string TAG = "[DropController]";

		private const int MAX_DROP_COUNT = 1;
		private const float MIN_DROP_LAY_TIME = 0.1f;
		private const float MAX_DROP_LAY_TIME = 0.2f;

		private HUDController hud;

		public DropController(HUDController hud)
		{
			this.hud = hud;
		}

		private Dictionary<Transform, (ItemCount, Sequence)> floatingItems = new Dictionary<Transform, (ItemCount, Sequence)>();

		public void DropItems(IEnumerable<ServerDrop> serverDrops, Vector3 fromWorldPoint = default, bool needUp = true)
		{
			if (serverDrops == null)
				return;

			if (fromWorldPoint == default)
				fromWorldPoint = Game.HUD.Content.transform.position;

			foreach (var serverDrop in serverDrops)
				DropItems(serverDrop.Items, fromWorldPoint, needUp);
		}

		public void DropItems(IEnumerable<ItemCount> itemCounts, Vector3 fromWorldPoint = default, bool needUp = true, int moneyPiles = 0)
		{
			if (fromWorldPoint == default)
				fromWorldPoint = Game.HUD.Content.transform.position;

			foreach (var itemCount in itemCounts)
			{
				DropItem(itemCount, fromWorldPoint, GetTargetPosition(itemCount.Item), needUp, moneyPiles: moneyPiles);
			}
		}
		
		public void DropFromWorld(ItemCount itemCount, Vector3 fromWorldPoint, bool needUp = true, bool bonus = false)
		{
			DropFromWorld(itemCount, fromWorldPoint, GetTargetPosition(itemCount.Item), needUp, bonus);
		}
		
		public void DropFromWorld(ItemCount itemCount, Vector3 fromWorldPoint, Vector3 toWorldPoint, bool needUp = true, bool bonus = false)
		{
			var final = hud.HudTopLayer.InverseTransformPoint(toWorldPoint);
			
			SpawnItemInner(itemCount, fromWorldPoint, final, needUp, bonus: bonus);
		}

		public void DropItem(ItemCount itemCount, Vector3 fromWorldPoint = default, bool needUp = true, bool bonus = false, int moneyPiles = 0)
        {
			DropItem(itemCount, fromWorldPoint, GetTargetPosition(itemCount.Item), needUp, bonus, moneyPiles);
		}

        public void DropItem(ItemCount itemCount, Vector3 fromWorldPoint, Vector3 toWorldPoint, bool needUp = true, bool bonus = false, int moneyPiles = 0)
        {
			if (fromWorldPoint == default)
				fromWorldPoint = Game.HUD.Content.transform.position;

            var holder = hud.HudTopLayer;
            var posStart = holder.InverseTransformPoint(fromWorldPoint);
            var final = holder.InverseTransformPoint(toWorldPoint);

			GameLogger.debug($"{TAG} Dropped {itemCount.Item.Name}; count = {itemCount.Count}");

			SpawnItemInner(itemCount, posStart, final, needUp, bonus: bonus);
        }

		public void DropItemsWithTransform(ItemCount itemCount, Transform transform, Vector3 fromWorldPoint, bool needUp = true)
		{
			Vector3 toWorldPoint = GetTargetPosition(itemCount.Item);

			var holder = hud.HudTopLayer;
			var posStart = holder.InverseTransformPoint(fromWorldPoint);
			var final = holder.InverseTransformPoint(toWorldPoint);

			GameLogger.debug($"{TAG} Dropped {itemCount.Item.Name}; count = {itemCount.Count}");

			SpawnItemInner(itemCount, transform, posStart, final, needUp);
		}

		public Sequence DropFakeItems(Image icon, Vector3 fromWorldPoint, Vector3 toWorldPoint, bool needUp = true)
        {
            var holder = hud.HudTopLayer;
            var posStart = holder.InverseTransformPoint(fromWorldPoint);
            var final = holder.InverseTransformPoint(toWorldPoint);
            
            var image = IconPlacer.SpawnIcon(icon, holder);
            return TweenItem(image.transform, posStart, final, needUp);
        }

		public IPromise DropClip(GameObject clip, Vector3 fromWorldPoint, Vector3 toWorldPoint, bool needUp = true)
		{
			var holder = hud.HudTopLayer;
			var posStart = holder.InverseTransformPoint(fromWorldPoint);
			var final = holder.InverseTransformPoint(toWorldPoint);

			clip.transform.SetParent(hud.HudTopLayer);
			return TweenItem(clip.transform, posStart, final, needUp).ToPromise();
		}

		private Vector3 GetTargetPosition(Item item)
		{
			var targetObj = GetTargetObj(item);
            return targetObj?.GetPositionGlobal() ?? Vector3.zero;
        }

		private IItemDropAnimated GetTargetObj(Item item)
		{
			if (item == Game.Static.Items.Money1)
				return Game.HUD.Content.MainScreen.Money1Panel;
			if (item == Game.Static.Items.Money2)
				return Game.HUD.Content.MainScreen.Money2Panel;

			return Game.HUD.Content.MainScreen.Money1Panel;
		}

        private void SpawnItemInner(ItemCount itemCount,
                                    Vector3 fromLocalPoint,
                                    Vector3 toLocalPoint,
                                    bool needUp = true,
                                    bool needTextWithCount = true,
                                    bool bonus = false)
        {
            if (Camera.main == null) 
				return;

            if (needTextWithCount)
            {
                var icon = CreateDropItemCount(itemCount, bonus);
                SpawnItemInner(itemCount, icon, fromLocalPoint, toLocalPoint, needUp);
            }
            else
            {
                IconPlacer.SpawnIcon(itemCount.Item, hud.HudTopLayer).Then(icon =>
                {
                    icon.raycastTarget = false;
                    SpawnItemInner(itemCount, icon.transform, fromLocalPoint, toLocalPoint, needUp);
                });
            }
        }

        private Transform CreateDropItemCount(ItemCount itemCount, bool bonus = false)
        {
            var result = Object.Instantiate(Game.BasePrefabs.DropItemCount, hud.HudTopLayer);
            result.ItemCount = itemCount;
			if (bonus)
				result.ItemName.text = "bonus".Localize();
			else
				result.DisableItemName();
            return result.transform;
        }

        private void SpawnItemInner(ItemCount itemCount, Transform icon, Vector3 fromLocalPoint, Vector3 toLocalPoint, bool needUp = true)
        {
            if (!icon) 
				return;

            Game.User.Items.OnDropStart(itemCount);
            var tweenSeq = TweenItem(icon, fromLocalPoint, toLocalPoint, needUp);
            floatingItems[icon] = (itemCount, tweenSeq);
        }
		
		public const float DROP_ITEM_UP_TIME = .4f;
		public const float DROP_ITEM_UP_RND_AMPLITUDE = .1f;

        public const float DROP_ITEM_TIME = 1f;
		public const float DROP_ITEM_RND_AMPLITUDE = .1f;

		public static Sequence TweenTransform(Transform clone, Vector3 fromLocalPoint, Vector3 toLocalPoint, bool needUp = true,
			bool needRandTime = true, float randomizeSpawn = 0, float speedUp = 1f, Vector2? bezierControlPoint = null, float? customDuration = null)
		{
			fromLocalPoint.Set(z: 0f);
			toLocalPoint.Set(z: 0f);
			clone.localPosition = fromLocalPoint;

			var firstTo = fromLocalPoint;

			var tweenSeq = DOTween.Sequence();
			tweenSeq.SetLink(Game.HUD.Content.gameObject);

			if (randomizeSpawn > 0)
			{
				firstTo = new Vector3(
					fromLocalPoint.x + Random.Range(-randomizeSpawn, randomizeSpawn),
					fromLocalPoint.y + Random.Range(-randomizeSpawn, randomizeSpawn),
					0f);
			}

			if (needUp)
			{
				firstTo = new Vector3(
					fromLocalPoint.x + Random.Range(-200f, 200f),
					fromLocalPoint.y + Random.Range(-200f, 0f),
					0f);

				var controlPointFirst = new Vector3(
					(fromLocalPoint.x + firstTo.x) / 2 + Random.Range(-100f, 100f),
					(fromLocalPoint.y + firstTo.y) / 2 + Random.Range(100f, 300f),
					0f);

				var firstTime = 0f;

				float duration;
				if (customDuration.HasValue)
					duration = customDuration.Value;
				else
					duration = needRandTime
					? Random.Range(DROP_ITEM_UP_TIME - DROP_ITEM_UP_RND_AMPLITUDE, DROP_ITEM_UP_TIME + DROP_ITEM_UP_RND_AMPLITUDE)
					: DROP_ITEM_UP_TIME;

				tweenSeq.Append(
								DOTween.To(() => firstTime, newTime => firstTime = newTime, 1f, duration)
									   .OnUpdate(() =>
										{
											if (clone != null)
												clone.localPosition =
																Bezier2(fromLocalPoint, controlPointFirst,
																		firstTo, firstTime);
										})
										.SetDelay(Random.Range(0f, 0.2f))
									    .SetLink(clone.gameObject)
										.SetLink(Game.HUD.Content.gameObject)
							   );
			}

			var time = 0f;
			if (!bezierControlPoint.HasValue)
				bezierControlPoint = new Vector3((firstTo.x + toLocalPoint.x) / 2, (firstTo.y + toLocalPoint.y) / 2 + Screen.height / Random.Range(5f, 10f), 0f);

			var durationMain = needRandTime
				? Random.Range(DROP_ITEM_TIME - DROP_ITEM_RND_AMPLITUDE, DROP_ITEM_TIME + DROP_ITEM_RND_AMPLITUDE)
				: DROP_ITEM_TIME;

			if (!speedUp.CloseTo(0))
				durationMain /= speedUp;

			tweenSeq.Append(
				DOTween.To(() => time, newTime => time = newTime, 1f, durationMain)
					.OnUpdate(OnUpdate)
					.SetEase(Ease.InQuad)
					.SetDelay(needUp ? Random.Range(MIN_DROP_LAY_TIME, MAX_DROP_LAY_TIME) : 0)
					.SetLink(Game.HUD.Content.gameObject)
					.SetLink(clone.gameObject)
			);

			void OnUpdate()
			{
				if (clone)
					clone.localPosition = Bezier2(firstTo, bezierControlPoint.Value, toLocalPoint, time);
			}

			return tweenSeq;
		}

		private Sequence TweenItem(Transform clone, Vector3 fromLocalPoint, Vector3 toLocalPoint, bool needUp = true, bool needRandTime = true,
			float randomizeSpawn = 0, float speedUp = 1f, Vector2? bezierControlPoint = null, bool isFake = false)
        {
			var tweenSeq = TweenTransform(clone, fromLocalPoint, toLocalPoint, needUp, needRandTime, randomizeSpawn, speedUp, bezierControlPoint);

			var onComplete = tweenSeq.onComplete;
            tweenSeq.OnComplete(() =>
			{
				onComplete?.Invoke();
				FinishFloatingItem(clone, !isFake);
			});

            return tweenSeq;
        }

        public static Vector3 Bezier2(Vector3 Start, Vector3 Control, Vector3 End, float t)
        {
			return ((1 - t) * (1 - t) * Start) + (2 * t * (1 - t) * Control) + (t * t * End);
        }

        private void FinishFloatingItem(Transform img, bool doFinishActions = true)
        {
            if (img == null)
				return;

            if (floatingItems.TryGetValue(img, out var value))
            {
                value.Item2?.Kill(false);
                floatingItems.Remove(img);

                if (value.Item1 != null && doFinishActions)
                {
                    Game.User.Items.OnDropFinish(value.Item1);
                }

				if (value.Item1?.Item != null)
				{
					var targetOpj = GetTargetObj(value.Item1.Item);
					targetOpj?.OnItemDropArrival();
				}
			}

            Object.Destroy(img.gameObject);
        }

        public void DisableFloating()
        {
            var keys = floatingItems.Keys.ToArray();

            foreach (var img in keys)
            {
                FinishFloatingItem(img, false);
            }
        }

        public float GetDroppedItemsCount(Item item)
        {
            var result = 0f;

            foreach (var obj in floatingItems.Values)
            {
                if (obj.Item1.Item == item)
                    result += obj.Item1.Count;
            }

            return result;
        }

		public void DropText(string text)
		{
			Game.HUD.Content.transform.ShowFloatingText(text);
		}

		public void Free()
		{
			DisableFloating();
		}
	}
}