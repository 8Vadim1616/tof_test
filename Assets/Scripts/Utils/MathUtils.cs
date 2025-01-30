using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Utils
{
    public static class MathUtils
    {
        public static Vector3 Bezier2(Vector3 Start, Vector3 Control, Vector3 End, float t)
        {
            return (1-t)*(1-t) * Start + 2 * t* (1 - t) * Control + t* t * End;
        }

        public static Vector3 Bezier3(Vector3 start, Vector3 control1, Vector3 control2, Vector3 end, float t)
        {
            var tm = 1 - t;
            return tm * tm * tm * start + 3 * t * tm * tm * control1 + 3 * t * t * tm * control2 + t * t * t * end;
        }
        
        public static T RandomFromCollection<T>(this IList<T> target) =>
            target?.Count > 0 ? target[Random.Range(0, target.Count)] : default(T);
        
        public static T Clamp<T>(this T target, T min, T max) where T : IComparable
        {
            if (target.CompareTo(min) < 0) return min;
            if (target.CompareTo(max) > 0) return max;
            return target;
        }

        // Находится ли значение между двумя другими
        public static bool Between<T>(this T target, float min, float max) where T : IComparable =>
            target.CompareTo(min) >= 0 && target.CompareTo(max) <= 0;

        public static bool BetweenUnordered<T>(this float target, float val1, float val2) =>
            target.Between(val1, val2) || target.Between(val2, val1);

        // Тоже что и кламп но не важно что минимум, а что максимум
        public static float ClampBetween(this float target, float val1, float val2)
        {
            return val1 > val2 ? Mathf.Clamp(target, val2, val1) : Mathf.Clamp(target, val1, val2);
        }

        // Не помню для чего это?
        public static float FloatTowards(float from, float towards, float max)
        {
            max = Mathf.Abs(max);
            var d = towards - @from;
            if (Mathf.Abs(d) <= max) return d;
            return (d > 0) ? max : -max;
        }

        // Рандом между двумя числами, не важно какое максимум
        public static float RandomBetween(float val1, float val2)
        {
            return val1 > val2 ? Random.Range(val2, val1) : Random.Range(val1, val2);
        }
        
        public static int RandomBetween(int val1, int val2)
        {
            return val1 > val2 ? Random.Range(val2, val1) : Random.Range(val1, val2);
        }

        public static bool CloseTo(this float val1, float val2, float? maxDist = null)
        {
            return Mathf.Abs(val1 - val2) <= (maxDist ?? Mathf.Epsilon);
        }

        /// <summary>
        /// То на сколько нужно умножить стороны target чтобы они вмещались внутри bounds.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="bounds"></param>
        /// <param name="keepAspectRatio"></param>
        /// <returns></returns>
        public static Vector2 GetScalesToFit(this Vector2 target, Vector2 bounds, bool keepAspectRatio = true)
        {
            var scaleX = target.x != 0 ? bounds.x / target.x : 0;
            var scaleY = target.y != 0 ? bounds.y / target.y : 0;

            if (!keepAspectRatio)
				return new Vector2(scaleX, scaleY);

            var scale = Mathf.Min(scaleX, scaleY);
            return new Vector2(scale, scale);
		}

		/// <summary>
		/// То на сколько нужно умножить стороны target чтобы они вмещались внутри bounds.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="bounds"></param>
		/// <param name="keepAspectRatio"></param>
		/// <returns></returns>
		public static Vector2 GetScalesToFill(this Vector2 target, Vector2 bounds, bool keepAspectRatio = true)
		{
			var scaleX = target.x != 0 ? bounds.x / target.x : 0;
			var scaleY = target.y != 0 ? bounds.y / target.y : 0;

			if (!keepAspectRatio)
				return new Vector2(scaleX, scaleY);

			var scale = Mathf.Max(scaleX, scaleY);
			return new Vector2(scale, scale);
		}

		/// <summary>
		/// То на сколько нужно умножить стороны target чтобы они вмещались внутри bounds.
		/// </summary>
		/// <param name="targetMin"></param>
		/// <param name="targetMax"></param>
		/// <param name="bounds"></param>
		/// <returns></returns>
		public static Vector2 GetScalesToFillWithMinZone(Vector2 targetMin, Vector2 targetMax, Vector2 bounds)
		{
			var scaleMinX = targetMin.x != 0 ? bounds.x / targetMin.x : 0;
			var scaleMinY = targetMin.y != 0 ? bounds.y / targetMin.y : 0;

			var scaleMaxX = targetMax.x != 0 ? bounds.x / targetMax.x : 0;
			var scaleMaxY = targetMax.y != 0 ? bounds.y / targetMax.y : 0;


			var scaleMin = Mathf.Min(scaleMinX, scaleMinY);
			var scaleMax = Mathf.Max(scaleMaxX, scaleMaxY);

			if (scaleMax > scaleMin)
				return new Vector2(scaleMax, scaleMax);

			return new Vector2(scaleMin, scaleMin);
		}

		/// <summary>
		/// Вовращает случайный элемент из коллекции, где для каждого элемента имеется своя вероятность выпадения.
		/// Если вероятность не указана в отдельном массиве она берется равной 1.
		/// Если вероятность указана, хотя бы у одного элемента она должна быть выше 0.
		/// </summary>
		public static T GetRandomWeightedElement<T>(this IList<T> target, IList<int> weights = null)
		{
			return target[target.GetRandomWeightedElementIndex(weights)];
		}

		/// <summary>
		/// Вовращает индекс случайного элемента из коллекции, где для каждого элемента имеется своя вероятность выпадения.
		/// Если вероятность не указана в отдельном массиве она берется равной 1.
		/// Если вероятность указана, хотя бы у одного элемента она должна быть выше 0.
		/// </summary>
		public static int GetRandomWeightedElementIndex<T>(this IList<T> target, IList<int> weights = null)
		{
			if (!(target?.Count > 0))
				throw new Exception("trying to get random element out of empty or null array");

			if (weights != null)
			{
				if (weights.Any(x => x < -1) == true)
					throw new Exception("Negative chances are not allowed");

				if (weights?.Count >= target.Count && weights.All(x => x == 0))
					throw new Exception("Trying to get a random element with 0 chances");
			}

			var listP = weights == null ? new List<int>() : weights.ToList();

			while (listP.Count < target.Count)
				listP.Add(1);

			for (int i = 1; i < listP.Count; i++)
				listP[i] += listP[i - 1];

			var random = Random.Range(0, listP.Last());

			for (int i = 0; i < listP.Count; i++)
				if (random < listP[i] &&
					(i == 0 && listP[i] > 0 || i > 0 && listP[i] > listP[i - 1])) // Проверяем что шанс не был нулевым
					return i;

			throw new Exception("Something went wrong with calc");
		}


		/// <summary>
		/// Вовращает случайный элемент из коллекции, где для каждого элемента имеется своя вероятность выпадения.
		/// Если вероятность не указана в отдельном массиве она берется равной 1.
		/// Если вероятность указана, хотя бы у одного элемента она должна быть выше 0.
		/// </summary>
		public static T GetRandomWeightedElement<T>(this IList<T> target, IList<float> weights = null)
		{
			return target[target.GetRandomWeightedElementIndex(weights)];
		}

		/// <summary>
		/// Вовращает индекс случайного элемента из коллекции, где для каждого элемента имеется своя вероятность выпадения.
		/// Если вероятность не указана в отдельном массиве она берется равной 1.
		/// Если вероятность указана, хотя бы у одного элемента она должна быть выше 0.
		/// </summary>
		public static int GetRandomWeightedElementIndex<T>(this IList<T> target, IList<float> weights = null)
		{
			if (!(target?.Count > 0))
				throw new Exception("trying to get random element out of empty or null array");

			if (weights != null)
			{
				if (weights.Any(x => x < 0) == true)
					throw new Exception("Negative chances are not allowed");

				if (weights?.Count >= target.Count && weights.All(x => x == 0))
					throw new Exception("Trying to get a random element with 0 chances");
			}

			var listP = weights == null ? new List<float>() : weights.ToList();

			while (listP.Count < target.Count)
				listP.Add(1);

			for (int i = 1; i < listP.Count; i++)
				listP[i] += listP[i - 1];

			var random = Random.Range(0f, listP.Last());

			for (int i = 0; i < listP.Count; i++)
				if (random <= listP[i] &&
					(i == 0 && listP[i] > 0 || i > 0 && listP[i] > listP[i - 1])) // Проверяем что шанс не был нулевым
					return i;

			throw new Exception("Something went wrong with calc");
		}
	}
}