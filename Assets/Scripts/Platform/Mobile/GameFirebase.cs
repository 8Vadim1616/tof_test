#if !UNITY_WSA && !UNITY_WEBGL && !BUILD_AMAZON && !BUILD_HUAWEI && !BUILD_CHINA
using System;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using Firebase;
using Firebase.Extensions;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile
{
    public class GameFirebase
    {
        private static GameFirebase _instance = null;
        public static GameFirebase Instance
        {
            get { return _instance ?? (_instance = new GameFirebase()); }
        }

        private Promise initPromise = null;

        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;

        public bool IsInitialized { get; private set; }

        public IPromise Init()
        {
            if (initPromise != null)
                return initPromise;

            initPromise = new Promise();
			var loadNativePromise = new Promise();

			Game.Instance.LoadNativeLibrariesPromise = Game.Instance.LoadNativeLibrariesPromise
				.Then(() =>
				{
					FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
					{
						dependencyStatus = task.Result;
						if (dependencyStatus == DependencyStatus.Available)
						{
							IsInitialized = true;
							initPromise.ResolveOnce();
							loadNativePromise.ResolveOnce();
						}
						else
						{
							Debug.LogError("[GameFirebase] Could not resolve all Firebase dependencies: " + dependencyStatus);

							initPromise.RejectOnce(new Exception("Could not resolve all Firebase dependencies: " + dependencyStatus));
							loadNativePromise.ResolveOnce();
						}
					});
				})
				.Then(() => loadNativePromise);

            return initPromise;
        }
    }
}
#endif
