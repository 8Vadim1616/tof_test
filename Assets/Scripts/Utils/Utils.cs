using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Assets.Scripts.Animations;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.UI.Windows;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleDiskUtils;
using Spine.Unity;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Utils
{
	public static class Utils
	{
		public static void SetPivot(this RectTransform rectTransform, Vector2 pivot)
		{
			Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
			deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
			deltaPosition.Scale(rectTransform.localScale);          // apply scaling
			deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation
    
			rectTransform.pivot = pivot;                            // change the pivot
			rectTransform.localPosition -= deltaPosition;           // reverse the position change
		}
		
		public static bool CheckAvailableSpace(string str, bool showWindow = true)
		{
			var needBytes = Encoding.UTF8.GetByteCount(str);
			return CheckAvailableSpace(needBytes, showWindow);
		}
		
		public static bool CheckAvailableSpace(long needBytes, bool showWindow = true)
		{
			if (Game.Settings == null)
				return true;

			if (!Game.Settings.CheckFreeSpace)
				return true;
			
			var availableSpace = DiskUtils.CheckAvailableSpace();
			var result = availableSpace > Mathf.CeilToInt(needBytes / 1024f / 1024f);
			if (result)
				return result;
			
			Debug.Log("No available disk space to save logs");
			if (showWindow && !InfoScreenExtensions.WasShowInCurrentSession && Game.Instance.IsLoaded.Value)
			{
				InfoScreenExtensions.WasShowInCurrentSession = true;
				InfoScreenExtensions.OfNoFreeSpace();
			}

			return result;
		}
		
		public static T Parse<T>(this object value)
		{
			try
			{
				if (!typeof(T).IsEnum)
					return (T) Convert.ChangeType(value, typeof(T));

				if (Enum.IsDefined(typeof(T), value)) // enum должен быть : long, а не int
					return (T) value;

				return default;
			}
			catch (Exception e)
			{
				return default;
			}
		}
		
		public static List<T> GetAllPublicStaticFieldsValues<T>(this Type type)
		{
			return type
				.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
				.Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
				.Select(x => (T)x.GetRawConstantValue())
				.ToList();
		}
		
		public static IPromise Fade(this GameObject gameObject, float toAlpha, float duration)
		{
			var allObjects = GetAllObjectsInTransformList(gameObject.transform);
			allObjects.Add(gameObject);

			var listImg = new List<Image>();
			var listSprite = new List<SpriteRenderer>();
			var listText = new List<TMP_Text>();
			var listAnim = new List<AbstractAnimationController>();
			var listSkeletonGraphics = new List<SkeletonGraphic>();
			var listSkeletonAnimation = new List<SkeletonAnimation>();

			float? fromAlpha = null;
			float? lastAlpha = null;
			var allSame = true;

			void CheckAlphaSame()
			{
				if (fromAlpha.HasValue && lastAlpha.HasValue && lastAlpha != fromAlpha) 
					allSame = false;

				lastAlpha = fromAlpha;
			}

			foreach (var obj in allObjects)
			{
				if (obj.TryGetComponent<Image>(out var image))
				{
					fromAlpha = image.color.a;
					listImg.Add(image);
					CheckAlphaSame();
				}

				var sprite = obj.GetComponent<SpriteRenderer>();
				if (sprite)
				{
					fromAlpha = sprite.color.a;
					listSprite.Add(sprite);
					CheckAlphaSame();
				}

				var text = obj.GetComponent<TMP_Text>();
				if (text)
				{
					fromAlpha = text.alpha;
					listText.Add(text);
					CheckAlphaSame();
				}

				var anim = obj.GetComponent<AbstractAnimationController>();
				if (anim)
				{
					fromAlpha = anim.Alpha;
					listAnim.Add(anim);
					CheckAlphaSame();
				}
				else
				{
					var skel = obj.GetComponent<SkeletonGraphic>();
					if (skel)
					{
						fromAlpha = skel.color.a;
						listSkeletonGraphics.Add(skel);
						CheckAlphaSame();
					}

					var skel2 = obj.GetComponent<SkeletonAnimation>();
					if (skel2)
					{
						fromAlpha = skel2.skeleton.A;
						listSkeletonAnimation.Add(skel2);
						CheckAlphaSame();
					}
				}
			}

			if (allSame && fromAlpha == toAlpha)
				return Promise.Resolved();
			
			var result = new Promise();
			var curAlpha = fromAlpha.Value;

			if (duration == 0)
			{
				SetAlpha(toAlpha);
				result.Resolve();
			}
			else
			{
				DOTween.To(() => curAlpha, val =>
				{
					curAlpha = val;
					SetAlpha(val);
				}, toAlpha, duration)
			   .OnComplete(result.Resolve)
			   .SetLink(gameObject);
			}

			void SetAlpha(float a)
			{
				foreach (var s in listSprite)
					s.color = s.color.SetAlpha(a);

				foreach (var text in listText)
					text.alpha = a;

				foreach (var anim in listAnim)
					anim.Alpha = a;

				foreach (var image in listImg)
					image.color = image.color.SetAlpha(a);

				foreach (var skelG in listSkeletonGraphics)
				{
					skelG.color = skelG.color.Set(a: a);
					skelG.Update(0);
					skelG.LateUpdate();
				}

				foreach (var skel in listSkeletonAnimation)
				{
					skel.skeleton.A = a;
				}

			}

			return result;
		}

		public static void SetMaskInteraction(this GameObject gameObject, SpriteMaskInteraction maskInteraction)
		{
			var allObjects = GetAllObjectsInTransformList(gameObject.transform);
			allObjects.Add(gameObject);
			
			foreach (var obj in allObjects)
			{
				var sprite = obj.GetComponent<SpriteRenderer>();
				if (sprite)
					sprite.maskInteraction = maskInteraction;
			}
		}

		public static void SetColor(this GameObject gameObject, Color color)
		{
			var allObjects = GetAllObjectsInTransformList(gameObject.transform);
			allObjects.Add(gameObject);

			foreach (var obj in allObjects)
			{
				var sprite = obj.GetComponent<SpriteRenderer>();
				if (sprite)
					sprite.color = color;
				
				var image = obj.GetComponent<Image>();
				if (image)
					image.color = color;

				var text = obj.GetComponent<TMP_Text>();
				if (text)
					text.color = color;

				var anim = obj.GetComponent<AbstractAnimationController>();
				if (anim)
					anim.Color = color;
			}
		}

		public static void ShowFloatingText(this Transform transform, string text, bool fadeIn = false)
		{
			var floatingText = Object.Instantiate(Game.BasePrefabs.PrefabFloatingText, transform);
			floatingText.FadeIn = fadeIn;
			floatingText.Text = text;
		}

		public static void ShowFloatingText(Vector3 position, string text, bool fadeIn = false)
		{
			var floatingText = Object.Instantiate(Game.BasePrefabs.PrefabFloatingText, Game.HUD.HudTopLayer);
			floatingText.FadeIn = fadeIn;
			floatingText.transform.position = position;
			floatingText.Text = text;
		}

		public static void ShowFloatingText(this Component component, string text)
		{
			component.transform.ShowFloatingText(text);
		}

		public static void SetAlpha(this GameObject gameObject, float alpha)
		{
			var allObjects = GetAllObjectsInTransformList(gameObject.transform);
			allObjects.Add(gameObject);

			foreach (var obj in allObjects)
			{
				var sprite = obj.GetComponent<SpriteRenderer>();
				if (sprite)
				{
					var color = sprite.color;
					color.a = alpha;
					sprite.color = color;
				}
				
				var image = obj.GetComponent<Image>();
				if (image)
				{
					var color = image.color;
					color.a = alpha;
					image.color = color;
				}

				var text = obj.GetComponent<TMP_Text>();
				if (text)
					text.alpha = alpha;

				var anim = obj.GetComponent<AbstractAnimationController>();
				if (anim)
					anim.Alpha = alpha;
			}
		}

		public static void SetDisableState(this GameObject gameObject, bool disabled)
		{
			if (disabled)
				SetDisabled(gameObject, false);
			else
				SetEnabled(gameObject);

			//var allObjects = GetAllObjectsInTransformList(gameObject.transform);
			//allObjects.Add(gameObject);

			//foreach (var obj in allObjects)
			//{
			//	if (obj.TryGetComponent(out Image img))
			//		img.material = gray ? Game.BasePrefabs.DisableMaterial : null;
			//}
		}

		public static IPromise PlayParticleAnimation(string resource,
			Transform transform,
			Vector3 position = default,
			float duration = 0f,
			Vector3 scale = default)
		{
			var animPrefab = Resources.Load<GameObject>(resource);
			var anim = GameObject.Instantiate(animPrefab, transform);
			if (scale != default)
				anim.transform.localScale = scale;
			anim.transform.localPosition = position;
			var particleSystem = anim.GetComponent<ParticleSystem>();
			var result = new Promise();
			if (duration.Equals(0f))
				duration = particleSystem.main.duration;
			DOVirtual.DelayedCall(duration,
				() =>
				{
					GameObject.Destroy(anim);
					result.Resolve();
				},false).SetLink(anim.gameObject);
			return result;
		}
		
		public static IPromise AlphaTween(this TMP_Text text, float toValue, float duration)
		{
			var result = new Promise();
			text.DOFade(toValue, duration)
				.SetLink(text.gameObject)
				.OnComplete(result.Resolve);
			return result;
		}
		
		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
			Func<TSource, TKey> selector)
		{
			if (source == null || source.Count() == 0)
				return default(TSource);

			return source.MinBy(selector, null);
		}

		public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
			Func<TSource, TKey> selector, IComparer<TKey> comparer)
		{
			if (source == null) throw new ArgumentNullException("source");
			if (selector == null) throw new ArgumentNullException("selector");
			comparer ??= Comparer<TKey>.Default;

			using (var sourceIterator = source.GetEnumerator())
			{
				if (!sourceIterator.MoveNext())
				{
					throw new InvalidOperationException("Sequence contains no elements");
				}
				var min = sourceIterator.Current;
				var minKey = selector(min);
				while (sourceIterator.MoveNext())
				{
					var candidate = sourceIterator.Current;
					var candidateProjected = selector(candidate);
					if (comparer.Compare(candidateProjected, minKey) < 0)
					{
						min = candidate;
						minKey = candidateProjected;
					}
				}
				return min;
			}
		}
		
		public static void ResizeSpriteToScreen(SpriteRenderer sprite, Camera theCamera, int fitToScreenWidth = 1, int fitToScreenHeight = 1)
		{        
			sprite.transform.localScale = new Vector3(1,1,1);
 
			var width = sprite.sprite.bounds.size.x;
			var height = sprite.sprite.bounds.size.y;
         
			var worldScreenHeight = theCamera.orthographicSize * 2.0f;
			var worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;
         
			if (fitToScreenWidth != 0)
			{
				var sizeX = new Vector2(worldScreenWidth / width / fitToScreenWidth,sprite.transform.localScale.y);
				sprite.transform.localScale = sizeX;
			}
         
			if (fitToScreenHeight != 0)
			{
				var sizeY = new Vector2(sprite.transform.localScale.x, worldScreenHeight / height / fitToScreenHeight);
				sprite.transform.localScale = sizeY;
			}
		}
		
		private static MonoBehaviour _mb = null;

		public static IPromise AsyncOperationToPromise(AsyncOperation operation)
		{
			if (operation.isDone) return Promise.Resolved();
			var result = new Promise();
			_mb.StartCoroutine(coroutine(operation, result));

			IEnumerator coroutine(AsyncOperation operation, Promise promise)
			{
				while (!operation.isDone)
					yield return null;

				promise.Resolve();
			}

			return result;
		}

		public static void AddOnce<T>(this List<T> list, T item)
		{
			if (!list.Contains(item))
				list.Add(item);
		}
		
		public static Color GetColorFromRGB(int rgb)
		{
			var r = (rgb >> 16) / 255f;
			var g = (rgb >> 8 & 0x00FF) / 255f;
			var b = (rgb & 0xFF) / 255f;
			
			return new Color(r,g,b);
		}

		public static void ForceRebuildLayoutOnNextFrame(this RectTransform transform)
		{
			IDisposable dis = null;

			dis = Observable.NextFrame().Subscribe(r =>
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(transform);

				dis?.Dispose();
			});
		}
		
		public static void SetMainContainer(MonoBehaviour mainContainer)
		{
			_mb = mainContainer;
		}

		public static float GetPercent(float curProgress, float minProgress, float maxProgress)
		{
			return Mathf.Clamp(curProgress * (maxProgress - minProgress) + minProgress, minProgress, maxProgress);
		}

		public static IPromise PlayAnimation(this Animator animator, string animation)
		{
			//var result = new Promise();

			//if (animator.parameters.Any(x => x.name == animation))
			//{
			//	animator.enabled = true;
			//	animator.Play(animation);

			//	IDisposable sub = null;
			//	sub = Observable.EveryUpdate().Subscribe(
			//					val =>
			//					{
			//						if (!animator.isActiveAndEnabled)
			//							animator.enabled = true;

			//						var state = animator.GetCurrentAnimatorStateInfo(0);
			//						if (!state.IsName(animation) || state.normalizedTime > state.length)
			//						{
			//							result.Resolve();
			//							sub?.Dispose();
			//						}
			//					}).AddTo(animator);
			//}
			//else
			//	result.Resolve();

			//return result;

			var animPromise = new Promise();
			animator.enabled = true;
			animator.Play(animation);
			StartCoroutine(WaitEndAnim());
			return animPromise;

			IEnumerator WaitEndAnim()
			{
				while (animator && !animator.GetCurrentAnimatorStateInfo(0).IsName(animation))
					yield return null;

				while (animator && animator.GetCurrentAnimatorStateInfo(0) is { } state && state.IsName(animation) && state.normalizedTime < 1f)
					yield return null;

				animPromise.ResolveOnce();
			}
		}

		public static void ClearTilesCache()
		{
			cachedNames.Clear();
		}

		private static Dictionary<TileBase, string> cachedNames = new Dictionary<TileBase, string>();

		public static string Name(this TileBase tile)
		{
			if (!cachedNames.TryGetValue(tile, out string name))
			{
				name = tile.name;
				cachedNames.Add(tile, name);
			}

			return name;
		}

		public static bool IsTilesEquals(this TileBase tile1, TileBase tile2)
		{
			if (!tile1 || !tile2) return false;

			return tile1.Name().Equals(tile2.Name());

			//return tile1.Equals(tile2);
		}

		/// <summary>
		/// Helps to convert Unity's Application.systemLanguage to a
		/// 2 letter ISO country code. There is unfortunately not more
		/// countries available as Unity's enum does not enclose all
		/// countries.
		/// </summary>
		/// <returns>The 2-letter ISO code from system language.</returns>
		public static string To2LetterISOCode(this SystemLanguage lang)
		{
			string res = "EN";
			switch (lang)
			{
				case SystemLanguage.Afrikaans: res = "AF"; break;
				case SystemLanguage.Arabic: res = "AR"; break;
				case SystemLanguage.Basque: res = "EU"; break;
				case SystemLanguage.Belarusian: res = "BY"; break;
				case SystemLanguage.Bulgarian: res = "BG"; break;
				case SystemLanguage.Catalan: res = "CA"; break;
				case SystemLanguage.Chinese: res = "ZH"; break;
				case SystemLanguage.Czech: res = "CS"; break;
				case SystemLanguage.Danish: res = "DA"; break;
				case SystemLanguage.Dutch: res = "NL"; break;
				case SystemLanguage.English: res = "EN"; break;
				case SystemLanguage.Estonian: res = "ET"; break;
				case SystemLanguage.Faroese: res = "FO"; break;
				case SystemLanguage.Finnish: res = "FI"; break;
				case SystemLanguage.French: res = "FR"; break;
				case SystemLanguage.German: res = "DE"; break;
				case SystemLanguage.Greek: res = "EL"; break;
				case SystemLanguage.Hebrew: res = "IW"; break;
				case SystemLanguage.Hungarian: res = "HU"; break;
				case SystemLanguage.Icelandic: res = "IS"; break;
				case SystemLanguage.Indonesian: res = "IN"; break;
				case SystemLanguage.Italian: res = "IT"; break;
				case SystemLanguage.Japanese: res = "JA"; break;
				case SystemLanguage.Korean: res = "KO"; break;
				case SystemLanguage.Latvian: res = "LV"; break;
				case SystemLanguage.Lithuanian: res = "LT"; break;
				case SystemLanguage.Norwegian: res = "NO"; break;
				case SystemLanguage.Polish: res = "PL"; break;
				case SystemLanguage.Portuguese: res = "PT"; break;
				case SystemLanguage.Romanian: res = "RO"; break;
				case SystemLanguage.Russian: res = "RU"; break;
				case SystemLanguage.SerboCroatian: res = "SH"; break;
				case SystemLanguage.Slovak: res = "SK"; break;
				case SystemLanguage.Slovenian: res = "SL"; break;
				case SystemLanguage.Spanish: res = "ES"; break;
				case SystemLanguage.Swedish: res = "SV"; break;
				case SystemLanguage.Thai: res = "TH"; break;
				case SystemLanguage.Turkish: res = "TR"; break;
				case SystemLanguage.Ukrainian: res = "UK"; break;
				case SystemLanguage.Unknown: res = "EN"; break;
				case SystemLanguage.Vietnamese: res = "VI"; break;
			}
			//		Debug.Log ("Lang: " + res);
			return res;
		}
		
		public static void SetEnabled(this GameObject gameObject, bool value, bool alpha = true, bool includeInactive = false)
		{
			if (!gameObject) 
				return;
			if (value)
				SetEnabled(gameObject, includeInactive);
			else
				SetDisabled(gameObject, alpha, includeInactive);
		}

		public static void SetEnabled(this Button button, bool enabled)
		{
			button.interactable = enabled;
			SetEnabled(button.gameObject, enabled, false);
		}

		public static void SetEnabled(GameObject gameObject, bool includeInactive = false)
		{
			var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive);
			var images = gameObject.GetComponentsInChildren<Image>(includeInactive);
			var skeletonGraphics = gameObject.GetComponentsInChildren<SkeletonGraphic>(includeInactive);

			foreach (var renderer in renderers)
				renderer.sharedMaterial = Game.BasePrefabs.DefaultSpriteMaterial;

			foreach (var image in images)
				image.material = null;

			foreach (var renderer in skeletonGraphics)
				renderer.material = Game.BasePrefabs.DefaultSpineMaterial;
		}

		public static void SetDisabled(GameObject gameObject, bool alpha = true, bool includeInactive = false)
		{
			if (!gameObject)
				return;
			
			var renderers = gameObject.GetComponentsInChildren<SpriteRenderer>(includeInactive);
			var images = gameObject.GetComponentsInChildren<Image>(includeInactive);
			var skeletonGraphics = gameObject.GetComponentsInChildren<SkeletonGraphic>(includeInactive);

			var needMaterial = alpha ? Game.BasePrefabs.DisableMaterialAlpha : Game.BasePrefabs.DisableMaterial;

			foreach (var renderer in renderers)
				renderer.sharedMaterial = needMaterial;

			foreach (var image in images)
				image.material = needMaterial;

			foreach (var renderer in skeletonGraphics)
				renderer.material = needMaterial;
		}
		
		public static string Localize(this string str, params string[] parameters)
		{
			return Game.Localize(str, parameters);
		}

		public static bool HasLocalize(this string str)
		{
			return Game.Localization.ContainsKey(str);
		}

		public static void RemoveAllChilds(this GameObject parent)
		{
			RemoveAllChilds(parent.transform);
		}

		public static void RemoveAllChilds(this Transform parent)
		{
			while (parent.childCount > 0)
			{
				var child = parent.GetChild(0);
				child.SetParent(null);
				GameObject.Destroy(child.gameObject);
			}
		}

		public static string Hash(string pSource)
		{
			using (MD5 lHash = MD5.Create())
			{
				byte[] lData = lHash.ComputeHash(Encoding.UTF8.GetBytes(pSource));
				StringBuilder lBuilder = new StringBuilder();
				for (int i = 0; i < lData.Length; i++)
					lBuilder.Append(lData[i].ToString("x2"));
				return lBuilder.ToString();
			}
		}

		public static bool IsDefault<T>(T o)
		{
			if (o == null) // => ссылочный тип или nullable
				return true;
			if (Nullable.GetUnderlyingType(typeof(T)) != null) // nullable, не null
				return false;
			var type = o.GetType();
			if (type.IsClass)
				return false;
			else           // => тип-значение, есть конструктор по умолчанию
				return Activator.CreateInstance(type).Equals(o);
		}

		public static void UpdateFields<T>(T reference, T target)
		{
			if (reference == null || target == null) return;

			foreach (var propertyInfo in typeof(T).GetProperties())
			{
				if (propertyInfo.CanRead && propertyInfo.CanWrite)
				{
					var val = propertyInfo.GetValue(reference, null);
					if (!IsDefault(val))
						propertyInfo.SetValue(target, val, null);
				}
			}

			foreach (var fieldInfo in typeof(T).GetFields())
			{
				if (fieldInfo.IsPublic || fieldInfo.IsFamily)
				{
					var val = fieldInfo.GetValue(reference);
					if (!IsDefault(val))
						fieldInfo.SetValue(target, fieldInfo.GetValue(reference));
				}
			}
		}

		public static void Populate<T>(this JToken value, T target) where T : class
		{
			using (var sr = value.CreateReader())
			{
				JsonSerializer.CreateDefault().Populate(sr, target); // Uses the system default JsonSerializerSettings
			}
		}

		private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static long ConvertToTimeStamp(this DateTime dateTime)
		{
			return (long)(dateTime - _epoch).TotalSeconds;
		}

		public static DateTime ConvertToDateTime(long timeStamp)
		{
			return _epoch.AddSeconds(timeStamp);
		}


		public static int ConvertVersion(string ver)
		{
			if (ver == null)
			{
				Debug.LogError("[Utlis] Version Error Is Null");
				return 0;
			}

			var num = 0;

			var splitted = ver.Split('.');

			var koef = Mathf.Max(0, 3 - splitted.Length);

			if (splitted.Length > 3)
				Debug.LogWarning("[Utlis] Version Format Too Long");

			try
			{
				for (var i = 0; i < splitted.Length && i < 3; i++)
					num += (int)Mathf.Pow(1000, splitted.Length - i - 1 + koef) * int.Parse(splitted[i]);
			}
			catch (Exception e)
			{
				Debug.LogError("[Utlis] Version Error Format: " + e.Message);
			}

			return num;
		}

		private static string validedVersion = null;
		public static bool CheckValidVersion(string ver)
		{
			if (validedVersion == ver) return true;

			if (ConvertVersion(ver) <= ConvertVersion(Application.version))
			{
				validedVersion = ver;
				return true;
			}

			return false;
		}

		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = Random.Range(0, n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}

			return list;
		}

		public static T RandomElement<T>(this IList<T> list, bool remove = false)
		{
			if (list.Count == 0)
				return default(T);

			var index = Random.Range(0, list.Count);
			var result = list[index];

			if (remove)
				list.RemoveAt(index);

			return result;
		}

		public static T RandomElement<T>(this IEnumerable<T> enumerable)
		{
			if (!enumerable.Any())
				return default(T);

			var index = Random.Range(0, enumerable.Count());
			var result = enumerable.ElementAtOrDefault(index);

			return result;
		}

		/// <summary>Случайный элемент последовательности с учетом веса (вероятности)</summary>
		public static T RandomElementByWeight<T>(this IEnumerable<T> sequence, Func<T, float> weightSelector)
		{
			float totalWeight = sequence.Sum(weightSelector);

			float itemWeightIndex = Random.value * totalWeight;
			float currentWeightIndex = 0;

			foreach (var item in from weightedItem in sequence select new { Value = weightedItem, Weight = weightSelector(weightedItem) })
			{
				currentWeightIndex += item.Weight;
				if (currentWeightIndex >= itemWeightIndex)
					return item.Value;
			}
			return default;
		}

		public static void ForEach<T>(this IList<T> list, Action<T, int> action) where T : class
		{
			for (int i = 0; i < list.Count; i++)
				action(list[i], i);
		}
        public static void ForEach<T>(this IList<T> list, Action<T> action) where T : class
        {
            foreach (var t in list)
                action(t);
        }

		public static int IndexOf<T>(this IList<T> list, Predicate<T> action)
		{
			for (int i = 0; i < list.Count; i++)
				if (action(list[i]))
					return i;
			return -1;
		}

		public static bool Empty<T>(this IEnumerable<T> enumerable)
		{
			return !enumerable.Any();
		}

		public static string Join<T>(this IEnumerable<T> enumerable, string separator, Func<T, string> convertFunction)
		{
			if (convertFunction == null)
				convertFunction = arg => arg.ToString();

			return String.Join(separator, enumerable.Select(convertFunction).ToArray());
		}

		/*public static IList<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
	{
		return listToClone.Select(item => (T)item.Clone()).ToList();
	}*/

		public static List<T> Clone<T>(this List<T> oldList)
		{
			return new List<T>(oldList);
		}

		public static Dictionary<T1, T2> Clone<T1, T2>(this Dictionary<T1, T2> oldDictionary)
		{
			return oldDictionary.ToDictionary(entry => entry.Key,
									   entry => entry.Value);
		}

		public static string SplitAndJoinCamelCase(this string s, char splitter = ' ')
		{
			if (String.IsNullOrEmpty(s))
				return s;

			var builder = new StringBuilder();
			builder.Append(s[0]);
			for (int i = 1; i < s.Length; i++)
			{
				if (Char.IsUpper(s[i]) && !Char.IsUpper(s[i - 1]))
					builder.Append(splitter);
				builder.Append(s[i]);
			}

			return builder.ToString();
		}

		public static IEnumerable<string> SplitCamelCase(this string s)
		{
			if (s == null)
				return null;

			if (s.Length == 0)
				return new [] { s };

			var result = new List<string>();

			for (int index = 0; index < s.Length;)
			{
				var builder = new StringBuilder();
				builder.Append(s[index++]);

				for (int i = index; i < s.Length; i++, index++)
				{
					if (Char.IsUpper(s[i]) && !Char.IsUpper(s[i - 1]))
						break;

					builder.Append(s[i]);
				}

				result.Add(builder.ToString());
			}

			return result;
		}

		public static TValue FirstOrDefaultValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> where)
		{
			foreach (var kv in dictionary)
			{
				if (where(kv))
					return kv.Value;
			}
			return default(TValue);
		}

		public static double Divide(this TimeSpan t1, TimeSpan t2)
		{
			return t1.Ticks / (double)t2.Ticks;
		}

		public static TimeSpan Multiply(this TimeSpan t1, int number)
		{
			return TimeSpan.FromTicks(t1.Ticks * number);
		}

		public static TimeSpan Multiply(this TimeSpan timeSpan, double factor)
		{
			if (double.IsNaN(factor))
			{
				throw new ArgumentException("Arg_CannotBeNaN");
			}

			// Rounding to the nearest tick is as close to the result we would have with unlimited
			// precision as possible, and so likely to have the least potential to surprise.
			double ticks = Math.Round(timeSpan.Ticks * factor);
			if (ticks > long.MaxValue || ticks < long.MinValue)
			{
				throw new OverflowException("Overflow_TimeSpanTooLong");
			}

			return TimeSpan.FromTicks((long)ticks);
		}

		public static bool IsMain(this Thread thread)
		{
			return Game.MainThread == thread;
		}

		public static TResult Safe<T, TResult>(this T obj, Func<T, TResult> action) where T : class
		{
			return action(obj);
		}

		public static void Safe<T>(this T obj, Action<T> action) where T : class
		{
			action(obj);
		}

		public static Vector3 WithoutY(this Vector3 vector)
		{
			return new Vector3(vector.x, vector.y, vector.z);
		}

		public static Vector2 WithoutY2(this Vector3 vector)
		{
			return new Vector2(vector.x, vector.z);
		}

		public static float Percent(this float percent)
		{
			return percent / 100f;
		}

		public static float Percent(this int percent)
		{
			return percent / 100f;
		}

		public static float ToPercent(this float koef)
		{
			return koef * 100f;
		}

		public static float ToPercent(this int koef)
		{
			return koef * 100f;
		}

		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		public static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}

		public static HashSet<T> ToHashSet<T>(
						this IEnumerable<T> source,
						IEqualityComparer<T> comparer = null)
		{
			return new HashSet<T>(source, comparer);
		}

		private static readonly char[] CharsToEscape = { '!', '*', '\'', '(', ')' };

        public static object AnimationControllerFactory { get; internal set; }

        /// <summary>
        /// Escapes a string according to the URL data string rules given in RFC 3986.
        /// </summary>
        /// <param name="value">The value to escape.</param>
        /// <returns>The escaped value.</returns>
        /// <remarks>
        /// The <see cref="Uri.EscapeDataString"/> method is <i>supposed</i> to take on
        /// RFC 3986 behavior if certain elements are present in a .config file.  Even if this
        /// actually worked (which in my experiments it <i>doesn't</i>), we can't rely on every
        /// host actually having this configuration element present.
        /// </remarks>
        public static string Escape(string value)
		{
			if (string.IsNullOrEmpty(value))
				return string.Empty;

			// Start with RFC 2396 escaping by calling the .NET method to do the work.
			// This MAY sometimes exhibit RFC 3986 behavior (according to the documentation).
			// If it does, the escaping we do that follows it will be a no-op since the
			// characters we search for to replace can't possibly exist in the string.
			StringBuilder lEscaped = new StringBuilder(Uri.EscapeDataString(value));

			// Upgrade the escaping to RFC 3986, if necessary.
			for (int i = 0; i < CharsToEscape.Length; i++)
				lEscaped.Replace(char.ToString(CharsToEscape[i]), Uri.HexEscape(CharsToEscape[i]));

			// Return the fully-RFC3986-escaped string.
			return lEscaped.ToString();
		}

		public static Promise Wait(float time)
		{
			var promise = new Promise();

			if (time == 0f)
				promise.ResolveOnce();
			else
			{
				if (_mb && _mb.gameObject.activeSelf)
					_mb.StartCoroutine(timer());
				else
					Debug.LogWarning("_mb is null or inactive");
			}

			return promise;

			IEnumerator timer()
			{
				yield return new WaitForSeconds(time);
				promise.ResolveOnce();
			}
		}

		public static IPromise Wait(float time, GameObject link, bool ignoreTimeScale)
		{
			var promise = new Promise();

			DOVirtual.DelayedCall(time, after, ignoreTimeScale).SetLink(link);

			return promise;

			void after()
			{
				promise.Resolve();
			}
		}

		public static void StartCoroutine(IEnumerator routine)
		{
			if (_mb != null)
				_mb.StartCoroutine(routine);
			else
				Debug.LogWarning("_mb is null");
		}

		public static IDisposable ForEachFrame(Action action)
		{
			return Observable.EveryUpdate().Subscribe(x => action());
		}

		public static IPromise NextFrame(int count = 1)
		{
			Promise promise = new Promise();

			if (count <= 0)
				promise.Resolve();
			else
			{
				IDisposable dis = null;
				dis = ForEachFrame(() =>
								   {
									   count--;

									   if (count <= 0)
									   {
										   dis?.Dispose();
										   promise.ResolveOnce();
									   }
								   });
			}

			return promise;
		}

		public static IDisposable ForEachTime(float time, Action action)
		{
			return Observable.Interval(TimeSpan.FromSeconds(time)).Subscribe(_ => action());
		}

		public static IDisposable ForEachSecond(Action action)
		{
			return ForEachTime(1, action);
		}
		
		public static List<GameObject> GetAllObjectsInTransformList(Transform trans, List<GameObject> result = null)
		{
			if (result == null)
				result = new List<GameObject>();

			foreach (Transform child in trans)
			{
				result.Add(child.gameObject);
				GetAllObjectsInTransformList(child, result);
			}

			return result;
		}

		public static Dictionary<string, GameObject> GetAllObjectsInTransform(Transform trans, string prefix = "_", Dictionary<string, GameObject> contentDictionary = null)
		{
			if (contentDictionary == null)
				contentDictionary = new Dictionary<string, GameObject>();

			if (prefix == null || trans.name.StartsWith(prefix))
			{
				contentDictionary.Add(trans.name, trans.gameObject);
			}

			foreach (Transform child in trans)
				GetAllObjectsInTransform(child, prefix, contentDictionary);

			return contentDictionary;
		}
		
		public static IPromise ToPromise(this TweenerCore<Vector3, Vector3, VectorOptions> tween)
		{
			var promise = new Promise();
			var onWasComplete = tween.onComplete;
			var onWasKill = tween.onKill;
			tween.OnComplete(() =>
			{
				onWasComplete?.Invoke();
				promise.ResolveOnce();
			});
			tween.OnKill(() =>
			{
				onWasKill?.Invoke();
				promise.RejectOnce();
			});
			return promise;
		}
		
		public static IPromise ToPromise(this TweenerCore<float, float, FloatOptions> tween)
		{
			var promise = new Promise();
			tween.OnComplete(promise.ResolveOnce);
			tween.OnKill(() => promise.RejectOnce());
			return promise;
		}

		public static IPromise ToPromise(this Tween tween)
		{
			var promise = new Promise();
			var onWasComplete = tween.onComplete;
			var onWasKill = tween.onKill;
			tween.OnComplete(() =>
			{
				onWasComplete?.Invoke();
				promise.ResolveOnce();
			});
			tween.OnKill(() =>
			{
				onWasKill?.Invoke();
				promise.RejectOnce();
			});
			return promise;
		}

		public static IPromise ToPromise(this Sequence sequence)
		{
			var promise = new Promise();
			var onWasComplete = sequence.onComplete;
			var onWasKill = sequence.onKill;
			sequence.OnComplete(() =>
			{
				onWasComplete?.Invoke();
				promise.ResolveOnce();
			});
			sequence.OnKill(() =>
			{
				onWasKill?.Invoke();
				promise.RejectOnce();
			});
			return promise;
		}

		public static IPromise ToPromise(this TweenerCore<Quaternion, Vector3, QuaternionOptions> tween)
		{
			var promise = new Promise();
			tween.OnComplete(promise.Resolve);
			return promise;
		}

		/// <summary> Получает значение из коллекции. Если в коллекции не существует ключа key, возвращает defValue </summary>
		public static V GetValue<K, V>(this Dictionary<K, V> dict, K key, V defValue)
		{
			if (dict.ContainsKey(key))
				return dict[key];
			return defValue;
		}

		public static void CenterToItem(MonoBehaviour item, ScrollRect scrollRect)
		{
			var child = item.GetComponent<RectTransform>();
			var scrollRectTransform = scrollRect.GetComponent<RectTransform>();
			var content = scrollRect.content;

			// Item is here
			var itemCenterPositionInScroll = GetWorldPointInWidget(scrollRectTransform, GetWidgetWorldPoint(child));
			// But must be here
			var targetPositionInScroll = GetWorldPointInWidget(scrollRectTransform, GetWidgetWorldPoint(scrollRect.viewport));
			// So it has to move this distance
			var difference = targetPositionInScroll - itemCenterPositionInScroll;
			difference.z = 0f;

			//clear axis data that is not enabled in the scrollrect
			if (!scrollRect.horizontal)
			{
				difference.x = 0f;
			}
			if (!scrollRect.vertical)
			{
				difference.y = 0f;
			}

			var normalizedDifference = new Vector2(
													difference.x / (content.rect.size.x - scrollRectTransform.rect.size.x),
													difference.y / (content.rect.size.y - scrollRectTransform.rect.size.y));

			var newNormalizedPosition = scrollRect.normalizedPosition - normalizedDifference;
			if (scrollRect.movementType != ScrollRect.MovementType.Unrestricted)
			{
				newNormalizedPosition.x = Mathf.Clamp01(newNormalizedPosition.x);
				newNormalizedPosition.y = Mathf.Clamp01(newNormalizedPosition.y);
			}

			scrollRect.normalizedPosition = newNormalizedPosition;
		}

		public static bool NotNull(this object obj) => obj != null;
		public static bool IsNull(this object obj) => obj == null;
		public static bool IsNullOrEmpty<T>(this IEnumerable<T> array) => array is null || !array.Any();

		private static Vector3 GetWidgetWorldPoint(RectTransform target)
		{
			//pivot position + item size has to be included
			var pivotOffset = new Vector3(
										(0.5f - target.pivot.x) * target.rect.size.x,
										(0.5f - target.pivot.y) * target.rect.size.y,
										0f);
			var localPosition = target.localPosition + pivotOffset;
			return target.parent.TransformPoint(localPosition);
		}
		private static Vector3 GetWorldPointInWidget(RectTransform target, Vector3 worldPoint)
		{
			return target.InverseTransformPoint(worldPoint);
		}

		public static T GetChildComponentByName<T>(this GameObject obj, string name) where T : Component
		{
			foreach (T component in obj.GetComponentsInChildren<T>(true))
			{
				if (component.gameObject.name == name) {
					return component;
				}
			}
			return null;
		}

		public static Sequence StartScaleAnimation(Transform trans, float min = 0.9f, float max = 1.0f, float time = .5f)
		{
			Sequence textFlash;
			textFlash = DOTween.Sequence();
			trans.localScale = new Vector3(min, min, min);
			textFlash.Append(trans.DOScale(max, time));
			textFlash.Append(trans.DOScale(min, time));
			textFlash.OnComplete(() => textFlash.Restart());
			return textFlash;
		}
		public static Sequence StartFadeAnimation(CanvasGroup image, float min = 0.5f, float max = 1.0f, float time = .5f)
		{
			Sequence textAlpha;
			textAlpha = DOTween.Sequence();
			image.alpha = min;
			textAlpha.Append(image.DOFade(max, time));
			textAlpha.Append(image.DOFade(min, time));
			textAlpha.OnComplete(() => textAlpha.Restart());
			return textAlpha;
		}
		
		public static T Shift<T>(this List<T> list)
		{
			if (list.Count > 0)
			{
				var result = list[0];
				list.RemoveAt(0);
				return result;
			}
			return default(T);
		}

		public static bool IsIdenticalContent<T>(this IList<T> firstList, IList<T> secondList)
		{
			if (firstList == null && secondList == null)
				return true;

			if (firstList == null || secondList == null)
				return false;

			if (firstList == secondList)
				return true;

			if (firstList.Count != secondList.Count)
				return false;

			foreach (T item in firstList)
				if (!secondList.Contains(item))
					return false;
			
			return true;
		}

		public static void ResolveOnce(this Promise p)
		{
			if (p.CurState == PromiseState.Pending)
				p.Resolve();
		}

		public static void ResolveOnce<T>(this Promise<T> p, T value)
		{
			if (p.CurState == PromiseState.Pending)
				p.Resolve(value);
		}

		public static void RejectOnce(this Promise p, Exception e = null)
		{
			if(p.CurState == PromiseState.Pending)
				p.Reject(e);
		}
		
		public static IPromise FadeAndDisable<T>(this T obj, bool active, float duration = .5f, bool createCanvas = true) where T : Component
		{
			return FadeAndDisable(obj.gameObject, active, duration, createCanvas);
		}

		public static IPromise FadeAndDisable(this GameObject go, bool active, float duration = .5f, bool createCanvas = true)
		{
			var result = new Promise();
			if (go.activeSelf == active)
			{
				result.Resolve();
				return result;
			}

			var sequence = DOTween.Sequence();
			var cg = go.GetComponent<CanvasGroup>();
			if (cg == null && !createCanvas)
			{
				result.Resolve();
				return result;
			}

			if (cg == null)
				cg = go.AddComponent<CanvasGroup>();

			//Если дестроили геймобжект в этом кадре
			if (cg == null)
			{
				result.Resolve();
				return result;
			}

			cg.alpha = active ? 0 : 1;
			var endValue = active ? 1 : 0;
			cg.interactable = false;
			if (active)
				go.SetActive(true);

			sequence
				.Append(cg.DOFade(endValue, duration))
				.OnComplete(() =>
				{
					if (!active)
						go.SetActive(false);

					cg.interactable = true;
					result.Resolve();
				})
				.SetLink(go);

			return result;
		}

		public static Tween DOFade(this RectTransform target, float endValue, float duration, float? startValue = null)
        {
            var cg = target.GetComponent<CanvasGroup>();
            if (!cg)
				cg = target.gameObject.AddComponent<CanvasGroup>();

			if (!cg) //Если магия Юнити сломается, gameObject будет уничтожен, но проверка этого не выдаст, и CanvasGroup не сможет создаться
				return DOTween.Sequence();

			if (startValue.HasValue)
				cg.alpha = startValue.Value;

			return cg.DOFade(endValue, duration);
		}
		
		public static void SetAlpha(this RectTransform target, float value)
		{
			if (!target.TryGetComponent(out CanvasGroup cg))
				cg = target.gameObject.AddComponent<CanvasGroup>();
			cg.alpha = value;
		}

		public static int GetIntVersion(string version)
		{
			int res = 0;
			int multiplier = 1;

			var arr = version.Split('.');

			for (var i = arr.Length - 1; i >= 0; i--)
			{
				res += int.Parse(arr[i]) * multiplier;
				multiplier *= 1000;
			}

			return res;
		}
		
		public static Vector3 GetMouseWorldPoint()
		{
			var mousePos = Input.mousePosition;
			if (mousePos.x < 0 || mousePos.x >= Screen.width || mousePos.y < 0 || mousePos.y >= Screen.height)
				return default;

			return Game.MainCamera.ScreenToWorldPoint(Input.mousePosition);
		}

		public static T ReadFile<T>(string fileName)
		{
			string destination = Application.persistentDataPath + "/" + fileName;
			FileStream file;

			if(File.Exists(destination))
				file = File.OpenRead(destination);
			else
			{
				Debug.LogWarning("[ReadFile] " + fileName + " not found");
				return default(T);
			}

			BinaryFormatter bf = new BinaryFormatter();

			object data = bf.Deserialize(file);
			file.Close();

			return (T) data;
		}

		public static void SaveFile(string fileName, object data)
		{
			string destination = Application.persistentDataPath + "/" + fileName;
			FileStream file;

			Debug.LogWarning($"Save file {destination}");

			if(File.Exists(destination))
				file = File.OpenWrite(destination);
			else
				file = File.Create(destination);

			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(file, data);
			file.Close();
		}
		
		public static void RemoveJson(string fileName)
		{
			RemoveFile(fileName + ".json");
		}

		public static void RemoveFile(string fileName)
		{
			string destination = Application.persistentDataPath + "/" + fileName;
			if (File.Exists(destination))
				File.Delete(destination);
		}

		public static bool CloneJson(string from, string to)
		{
			return CloneFile(from + ".json", to + ".json");
		}

		public static bool CloneFile(string from, string to)
		{
			var fromDist = Application.persistentDataPath + "/" + from;
			var toDist = Application.persistentDataPath + "/" + to;
			if (!File.Exists(fromDist))
				return false;

			File.Copy(Application.persistentDataPath + "/" + from,
				Application.persistentDataPath + "/" + to);
			return true;
		}
		
		public static bool IsJsonExists(string fileName)
		{
			return IsFileExists(fileName + ".json");
		}

		public static bool IsFileExists(string fileName)
		{
			string destination = Application.persistentDataPath + "/" + fileName;

			return File.Exists(destination);
		}

		public static JToken ToJTokenForLogs(this object o)
		{
			NoLogsConverter.UseForConvert = false;
			var token = JToken.FromObject(o);
			NoLogsConverter.UseForConvert = true;
			return token;
		}

		public static T[] GetRange<T>(this T[] array, int index, int count)
		{
			T[] tmpArray = new T[count];
			Array.Copy(array, index, tmpArray, 0, count);
			return tmpArray;
		}

		public static string GetURL(string url, Dictionary<string, string> parameters) =>
			$"{url}?{string.Join("&", parameters.Keys.Select(key => $"{key}={Escape(parameters[key])}"))}";

		public static bool TryGetDictionaryValueMoreOrEqual<T>(Dictionary<int, T> dictionary, int forValue, out int keyValue, out T resultValue)
		{
			if (dictionary == null || dictionary.Count == 0)
			{
				keyValue = default;
				resultValue = default;
				return false;
			}

			if (dictionary.TryGetValue(forValue, out T outValue))
			{
				keyValue = forValue;
				resultValue = outValue;
				return true;
			}

			int lastClosestValue = int.MinValue;
			foreach (var key in dictionary.Keys)
				if (key <= forValue && key > lastClosestValue)
					lastClosestValue = key;

			if (lastClosestValue == int.MinValue)
			{
				keyValue = default;
				resultValue = default;
				return false;
			}

			keyValue = lastClosestValue;
			resultValue = dictionary[lastClosestValue];
			return true;
		}

		public static void ReplaceAtSmart(this List<float> target, int index, float value)
		{
			if (target == null)
				return;

			if (target.Count <= index)
				for (int i = index - target.Count + 1; i > 0; i--)
					target.Add(0f);

			target[index] = value;
		}

		public static string GetEnumLabel<EnumType>(EnumType feature) where EnumType : Enum
		{
			string label = feature.ToString();
			var enumType = typeof(EnumType);
			var memberInfo = enumType.GetMember(label).FirstOrDefault();
			var valueAttribute = memberInfo?.GetCustomAttributes(typeof(InspectorNameAttribute), false)
							.FirstOrDefault() as InspectorNameAttribute;
			if (valueAttribute != null)
				label = valueAttribute.displayName;

			return label;
		}
	}
}