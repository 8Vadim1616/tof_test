using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using DG.Tweening;

namespace Assets.Scripts.UI.Utils
{
    public static class PromiseUtils
    {
        [Obsolete]
        public static Promise AddToList(this Promise p, List<Promise> list)
        {
            list?.Add(p);
            return p;
        }

        public static List<Promise> CancelAll(this List<Promise> list)
        {
            if (list == null) return null;

            foreach (var promise in list)
            {
                if (promise != null && promise.CurState == PromiseState.Pending) promise.Reject(new Exception("Canceled"));
            }

            return list;
        }

        public static Promise CatchException(this Promise p)
        {
            p?.Catch(x => { });
            return p;
        }

        [Obsolete]
        public static Promise<T> AddToList<T>(this Promise<T> p, List<Promise<T>> list)
        {
            list?.Add(p);
            return p;
        }

        public static List<Promise<T>> CancelAll<T>(this List<Promise<T>> list)
        {
            if (list == null) return null;

            foreach (var promise in list)
            {
                if (promise != null && promise.CurState == PromiseState.Pending) promise.Reject(new Exception("Canceled"));
            }

            return list;
        }

        public static Promise<T> CatchException<T>(this Promise<T> p)
        {
            p?.Catch(x => {});
            return p;
        }

        public static IPromise WaitPromise(float time, bool ignoreTimeScale)
        {
            var p = new Promise();
			if (time.Equals(0f))
			{
				if (p.CurState == PromiseState.Pending)
					p.Resolve();
				return p;
			}

			DOVirtual.DelayedCall(time, () =>
            {
                if (p.CurState == PromiseState.Pending)
					p.Resolve();
            },ignoreTimeScale);
            return p;
        }

    }
}