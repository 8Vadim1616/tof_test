using System;
using Assets.Scripts.Static.Items;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.General
{
	public class ItemCountView : MonoBehaviour
	{
		protected const string BASE_PATH = "img/";

		public enum ItemCountStyle
		{
			COMMON = 0,
			X = 1,
			PLUS = 2,
			SLASH = 3,
			INFINITY = 4,
			X_ON_TAPE = 5,
			COLORED = 6,
			PERCENT = 7,
		}
		
		public enum ItemColorStyle
		{
			COMMON = 0,
			COLORED =1,
		}

		public ItemCountStyle CountStyle = ItemCountStyle.COMMON;

		public Image icon;
		public TextMeshProUGUI count;

		public bool HideIfNull = false;
		public bool NoCountIfOne = false;

		protected Item _item;
		protected long _count;
		protected long _count2;
		protected string _replaceIconPath;
		
		public Item Item
		{
			get => _item;
			set => SetItem(value);
		}

		private Transform extra;

		private IDisposable _itemCountSub;
		
		public virtual IPromise<Sprite> SetItem(Item item, string replaceIconPath = null)
		{
			Promise<Sprite> promise = new Promise<Sprite>();

			if (extra)
				Destroy(extra.gameObject);

			if (_item != item)
				_replaceIconPath = replaceIconPath;

			if (item == null)
			{
				if (icon && HideIfNull)
					icon.SetActive(false);

				promise.Resolve(null);
			}
			else
			{
				if (icon)
				{
					icon.SetActive(true);
					icon.enabled = true;

					if (_item != item)
					{
						//var wasAlphaIcon = icon.color.a;
						IPromise<Sprite> loadPromise = null;

						if (_replaceIconPath != null)
						{
							var path = _replaceIconPath.StartsWith(BASE_PATH)
								? _replaceIconPath
								: BASE_PATH + _replaceIconPath;
							loadPromise = icon.LoadFromAssets(path, needCache: true);
						}
						else
							loadPromise = icon.LoadItemImage(item, needCache: true);

						loadPromise
							.Then(sprite =>
							{
								promise.Resolve(sprite);

								//if (icon)
								//	icon.color = icon.color.SetUI(a: wasAlphaIcon);

								UpdateCountVisibility();
							});
					}
					else
					{
						promise.Resolve(icon.sprite);
					}
				}
			}
			
			_item = item;
			
			if (CountStyle == ItemCountStyle.COLORED && _item != null)
			{
				_itemCountSub?.Dispose();
				if (item == Game.Static.Items.WaveCoin)
				{
					_itemCountSub = Game.Instance.Playfiled.Value.Player.WaveCoin.Subscribe(_ =>
					{
						UpdateItemCount();
					}).AddTo(gameObject);
				}
				_itemCountSub = item.UserReactive().Subscribe(_ =>
				{
					UpdateItemCount();
				}).AddTo(gameObject);
			}
			
			return promise;
		}

		public long Count
		{
			get => _count;
			set
			{
				_count = value;
				UpdateItemCount();
			}
		}

		public long Count2
		{
			get => _count2;
			set
			{
				_count2 = value;
				UpdateItemCount();
			}
		}

		public virtual void SetCount(long c)
		{
			_count = c;
			UpdateItemCount();
		}

		protected virtual void UpdateItemCount()
		{
			if (!this)
				return;

			if (count != null)
			{
				count.text = GetCountText();

				if (count.transform.parent is RectTransform rectTransform)
					LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
			}

			UpdateCountVisibility();
		}

		protected string GetCountText()
		{
			switch (CountStyle)
			{
				case ItemCountStyle.COLORED:
					return TextFormatting.GetColoredCount(_item, _count);
				case ItemCountStyle.PLUS:
					return $"+{_count.ToKiloFormat()}";
				case ItemCountStyle.X:
				case ItemCountStyle.X_ON_TAPE:
					return $"x{_count.ToKiloFormat()}";
				case ItemCountStyle.SLASH:
					return $"{_count.ToKiloFormat()}/{_count2.ToKiloFormat()}";
				case ItemCountStyle.INFINITY:
					return "∞";
				case ItemCountStyle.PERCENT:
					return $"+{_count.ToKiloFormat()}%";
				case ItemCountStyle.COMMON:
				default:
					return _count.ToKiloFormat();
			}
		}


		protected virtual void UpdateCountVisibility()
		{
			if (count != null)
			{
				var hideAmount = _count <= 1 && NoCountIfOne;
				var hideNull = Item == null && HideIfNull;

				count.SetActive(!hideNull && !hideAmount);
			}
		}

		public ItemCount GetItemCount() => new ItemCount(Item, Count, _replaceIconPath);

		public ItemCount ItemCount
		{
			set => SetItemCount(value);
		}

		public virtual IPromise<Sprite> SetItemCount(ItemCount value, string replaceIconPath = null)
		{
			value = value.CheckAlternatives();

			Promise<Sprite> promise = new Promise<Sprite>();

			if (replaceIconPath != null)
				_replaceIconPath = replaceIconPath;
			else
				_replaceIconPath = value?.ReplaceIconPath;

			if (value?.Item == null)
			{
				if (count)
				{
					if (HideIfNull)
						count.gameObject.SetActive(false);
					else
						count.text = string.Empty;
				}

				if (icon)
				{
					if (HideIfNull)
						icon.gameObject.SetActive(false);
					else
						icon.sprite = null;
				}

				Item = null;
				_replaceIconPath = null;
				promise.Resolve(null);
			}
			else
			{
				SetItem(value.Item, _replaceIconPath)
					.Then(sprite => promise.Resolve(sprite));

				SetCount(value.Count);
			}

			return promise;
		}

		public void SetItemWithoutImage(Item item)
		{
			_item = item;
		}

		public void ForceCountRebuildLayout()
		{
			if (count.transform.parent is RectTransform rectTransform)
				LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
		}

		public void RemoveReplaceIcon() => _replaceIconPath = null;

		private Tween textAlpha;
		private const float TEXT_ALPHA_TIME = .25f;
		public void SetAlphaText(float alpha, bool instant = false)
		{
			if (!count || count.alpha.CloseTo(alpha))
				return;

			textAlpha?.Kill();

			if (instant)
			{
				count.alpha = alpha;
				return;
			}

			var dist = alpha - count.alpha;
			var time = Math.Abs(dist) * TEXT_ALPHA_TIME;

			textAlpha = count.DOFade(alpha, time);
		}


		private float alpha = 1f;
		public float Alpha
		{
			get => alpha;
			set
			{
				alpha = value;
				count.alpha = value;
				icon.color = icon.color.SetAlpha(value);
			}
		}

		public Tween SetAlphaAnimation(float a, float time = .25f, float? changeStart = null)
		{
			var tween = DOTween.To(() => Alpha, x => Alpha = x, a, time).SetLink(gameObject);

			if (changeStart != null) tween.ChangeStartValue(changeStart.Value);

			return tween;
		}

		public virtual Tween SetCountAnimation(int start, int end, float time = .25f)
		{
			if (!this || !gameObject)
				return DOTween.Sequence();
			
			Count = start;
			UpdateItemCount();

			float cur = start;
			int last = start;

			Tween t = null;

			var tween = DOTween.To(() => cur, Setter, end, time)
				.SetEase(Ease.OutQuad)
				.SetLink(gameObject);

			return tween;

			void Setter(float x)
			{
				var round = Mathf.RoundToInt(x);

				if (last != round)
					BounceTweenStart();

				Count = round;
				last = round;
			}

			void BounceTweenStart()
			{
				if (t?.active == true)
					return;

				if (!this || !count)
					return;

				t?.Kill();
				var wasScale = count.transform.localScale;
				var scaleTime = .1f;
				t = DOTween.Sequence()
						   .Append(count.transform.DOScale(wasScale * 1.1f, scaleTime))
						   .Append(count.transform.DOScale(wasScale, scaleTime))
						   .SetLink(count.gameObject)
						   .SetEase(Ease.InOutQuad);
			}
		}
	}
}