using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

// Небольшие утилиты для скорости работы
namespace Assets.Scripts.Utils
{
    public static class Util 
    {
		// форматирование с запятыми 1,000,000
		public static string FormatLikeMoney(this int amount) =>
			string.Format(CultureInfo.InvariantCulture, "{0:#,##0.##}", amount);

		public static bool Has<T>(this IEnumerable<T> target, T element) where T : System.IComparable<T>
        {
            return target.Any(t => t.CompareTo(element) == 0);
        }
        public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);

        #region Unity hierarchy utils
        public static List<(GameObject, MatchCollection)> FindInChildrenByName(this Transform t, string regularEx, bool recursive = false)
        {
            var regEx = new Regex(regularEx);

            var result = new List<(GameObject, MatchCollection)>();

            for (var i = 0; i < t.childCount; i++)
            {
                var child = t.GetChild(i);
                if (!child) continue;

                if (regEx.IsMatch(child.name))
                {
                    result.Add((child.gameObject, regEx.Matches(child.name)));
                }

                if (recursive) 
                    result.AddRange(child.FindInChildrenByName(regularEx, true));
            }

            return result;
        }

        /// <summary>
        /// На самом деле итеративный, без рекурсии
        /// </summary>
        /// <param name="t">трансформа родитель с детей которой начинается поиск</param>
        /// <param name="name">имя искомой траснформы</param>
        /// <returns>найденная трансформа или нулл</returns>
        public static Transform FindInChildrenRecursively(this Transform t, string name)
        {
            var queue = new Queue<Transform>();
            queue.Enqueue(t);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var res = current.Find(name);

                if (res) return res;

                current.ForeachChild(x => queue.Enqueue(x));
            }

            return null;
        }

        public static List<(GameObject, Match)> FindAllInChildrenRecursivelyByRegEx(this Transform t, string regularEx,
            Func<string, string> nameChanger = null)
        {
            var regEx = new Regex(regularEx);

            var result = new List<(GameObject, Match)>();

            t.ForeachChildrenRecursively(child =>
            {
                if (!child) return;

                var childName =nameChanger?.Invoke(child.name) ?? child.name;
                var match = regEx.Match(childName);
                if (match.Success) result.Add((child.gameObject, match));
            });

            return result;
        }

        public static T GetComponentInParents<T>(this GameObject gameObject)
        {
            return GetComponentInParents<T>(gameObject.transform);
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject) return null;

            var t = gameObject.GetComponent<T>();
            if (!t) t = gameObject.AddComponent<T>();
            return t;
        }


        public static T GetComponentInParents<T>(this Transform start)
        {
            while (start != null)
            {
                var comp = start.GetComponent<T>();
                if (comp != null) return comp;
                start = start.parent;
            }

            return default;
        }

        public static List<Transform> GetChildren(this Transform t)
        {
            var r = new List<Transform>();
            for (var i = 0; i < t.childCount; i++) r.Add(t.GetChild(i));
            return r;
        }

        public static void ForeachChildrenRecursively(this Transform t, Action<Transform> action)
        {
            if (t == null) return;
            action(t);
            for (var i = 0; i < t.childCount; i++)
                t.GetChild(i).ForeachChildrenRecursively(action);
        }

        public static void ForeachParentRecursively(this Transform t, Action<Transform> action)
        {
            if (t == null || t.parent == null) return;
            action(t.parent);
            t.parent.ForeachParentRecursively(action);
        }

        public static void ForeachChild(this Transform t, Action<Transform> a)
		{
			var children = new List<Transform>();
            for (var i = 0; i < t.childCount; i++)
            {
                var c = t.GetChild(i);
				children.Add(c);
            }

			foreach (var c in children)
			{
				if (c) a(c);
			}
        }

        public static Transform DestroyAllChildren(this Transform target)
		{
			if (!target)
				return target;

			foreach (Transform t in target)
            {
                Object.Destroy(t.gameObject);
            }

            return target;
        }

        public static void SetActive(this Component m, bool active)
        {
            if (m && m.gameObject)
                m.gameObject.SetActive(active);
        }

        public static void EditorDestroy(this GameObject obj)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (obj != null) Object.DestroyImmediate(obj);
            };
#endif
        }

        #endregion

        public static TimeSpan SecondsToTimeSpan(this long time) => TimeSpan.FromSeconds(time);

        public static string DaysAgo(this TimeSpan time)
        {
            var days = time.Days;
            var dayStr = "day_many";
            if (days <= 1) dayStr = "day_one";
            else if (days <= 4) dayStr = "day_four";
            return dayStr.Localize();
        }

        /// <summary>
        /// Попарно переберает элементы двух коллекций и вызывает для каждого метод
        /// если парного элемента нет то либо вызывает метод для значения default либо игнорирует
        /// </summary>
        /// <typeparam name="T">Тип для коллекции 1</typeparam>
        /// <typeparam name="V">Тип для коллеции 2</typeparam>
        /// <param name="action">Вызываемый для элементов попарно метод</param>
        /// <param name="invokeIfNone">Следует ли вызывать метод в случае если парного элемента нет</param>
        public static void Pairwise<T, V>(this IList<T> listT, IList<V> listV, Action<T, V> action, bool invokeIfNone = true) 
        {
            for (var i = 0; i < listT.Count; i++)
            {
                if (!invokeIfNone  && listV.Count <= i) break;

                action?.Invoke(listT[i], listV.Count > i ? listV[i] : default(V));
            }

            if (!invokeIfNone) return;
            for (var i = listT.Count; i < listV.Count; i++)
            {
                action?.Invoke(default(T), listV[i]);
            }
        }

        /// <summary>
        /// Проверяет блокирует ли ЮИ указатель на экране. Проверяет мышь если это пк или касание если это планшет или смартфон 
        /// </summary>
        /// <returns> Находится ли указатель поверх элемента интерфейса </returns>
        public static bool CheckIfUIBlocksControls()
        {
            var eventSystem = EventSystem.current;
            if (!eventSystem) return false;

            switch (SystemInfo.deviceType)
            {
                case DeviceType.Desktop:
                   return eventSystem.IsPointerOverGameObject();
                case DeviceType.Handheld when Input.touchCount <= 0:
                    return false;
                case DeviceType.Handheld:
                {
                    var res = IsPointerOverUIObject();
                    return res;
                }
            }

            return false;
        }

        private static List<RaycastResult> raycastResults = new List<RaycastResult>();
        private static PointerEventData eventDataCurrentPosition = null;

        /// <summary>
        /// Истина если поверх ЮИ обьекта
        /// </summary>
        /// <param name="ignoreNonBlocking"> игнорировать ли обьекты в неблокирующем слою </param>
        /// <returns></returns>
        public static bool IsPointerOverUIObject(bool ignoreNonBlocking = false)
        {
            if (eventDataCurrentPosition == null)
                eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            
            EventSystem.current.RaycastAll(eventDataCurrentPosition, raycastResults);

            if (ignoreNonBlocking)
            {
                const int UINonBlockingLayer = 8;
                for (var i = 0; i < raycastResults.Count; i++)
                    if (raycastResults[i].gameObject.layer != UINonBlockingLayer)
                        return true;

                return false;
            }

            return raycastResults.Count > 0;
        }

		/// <summary>
		/// Истина если поверх ЮИ обьекта
		/// </summary>
		/// <param name="ignoreNonBlocking"> игнорировать ли обьекты в неблокирующем слою </param>
		/// <returns></returns>
		public static bool IsPointerOverBlockSwipeObject()
		{
			if (eventDataCurrentPosition == null)
				eventDataCurrentPosition = new PointerEventData(EventSystem.current);

			eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

			EventSystem.current.RaycastAll(eventDataCurrentPosition, raycastResults);

			const int BlockSwipeLayer = 9;
			for (var i = 0; i < raycastResults.Count; i++)
				if (raycastResults[i].gameObject.layer == BlockSwipeLayer)
					return true;

			return false;
		}

		public static List<RaycastResult> RaycastAllAtMousePos()
        {
            if (eventDataCurrentPosition == null)
                eventDataCurrentPosition = new PointerEventData(EventSystem.current);

            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            EventSystem.current.RaycastAll(eventDataCurrentPosition, raycastResults);

            return raycastResults;
        }

        public static Vector3 MousePosition(this Camera cam) => cam.ScreenToWorldPoint(Input.mousePosition);

        [Obsolete]
        public static bool CheckIfUIBlocksControlsIgnoreNonBlocking()
        {
            return IsPointerOverUIObject(true);

            var eventSystem = EventSystem.current;
            if (eventSystem == null) return false;

            const int UINonBlockingLayer = 8;
            if (CheckIfUIBlocksControls())
            {
                PointerEventData pointerData = new PointerEventData(eventSystem)
                {
                    pointerId = -1, 
                    position = Input.mousePosition,
                };

                eventSystem.RaycastAll(pointerData, raycastResults);

                if (raycastResults.Any(x => x.gameObject.layer != UINonBlockingLayer)) return true;
                return false;
            }

            return false;
        }
    }
}
