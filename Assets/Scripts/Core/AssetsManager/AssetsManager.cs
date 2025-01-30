using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Animations;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network;
using Assets.Scripts.UI.Utils;
using Assets.Scripts.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Core.AssetsManager
{
    public class AssetsManager : Singleton<AssetsManager>
    {
        public static string PARTS_URL = "";
        public static string ResourcesLoadServerPath = "";//"https://develop2.playme8.ru/farm3/assets"; // Нужно переприсвоить до того как будет запрошен первый адрессабл
        public static string [] PreloadCatalogs = {};

        public static bool NeedLog = false;
        public AssetsLoader Loader { get; private set; }
        public LocalFileLoader LocalLoader { get; private set; }
        
        public static string GetGlobalURL(string relativePath) => PARTS_URL + relativePath;
        
        private void Awake()
        {
            Loader = new AssetsLoader(this);
            LocalLoader = new LocalFileLoader();
        }

        public Promise<long> GetDownloadSizeAsync(string path)
        {
            var result = new Promise<long>();
            var operationHandle = Addressables.GetDownloadSizeAsync(path);

            StartCoroutine(coroutine(operationHandle, result));

            IEnumerator coroutine(AsyncOperationHandle<long> handle, Promise<long> promise)
            {
                while (!handle.IsDone)
                    yield return null;

                if (handle.Status == AsyncOperationStatus.Succeeded)
                    promise.Resolve(handle.Result);

                promise.Resolve(-1);    // nothing founded
            }

            return result;
        }

        public T CreateSync<T>(string path, Transform parent = null)
            where T : UnityEngine.Object
        {
            var prefab = Loader.LoadSync<T>(path);
            if (!prefab) return null;

            return Instantiate(prefab, parent);
        }

        /**
         * Для отрисовки в интерфейсах
         */
        public IPromise<AbstractAnimationController> CreateSpineGraphic(string path,
            RectTransform parent = null,
            string anim = null,
            bool fit = true)
        {
            var result = new Promise<AbstractAnimationController>();
            Loader.Load<SkeletonDataAsset>(path)
                .Then(asset =>
                    {
                        var newSkeletonAnimation = SkeletonGraphic.NewSkeletonGraphicGameObject(asset, parent, Game.BasePrefabs.DefaultSpineMaterial);
                        newSkeletonAnimation.gameObject.name = path;
                        var animationController = newSkeletonAnimation.gameObject.CreateAnimationController();
                        var animData = new AnimationData(anim);
                        if (anim != null && animationController.HasAnimation(animData))
                            animationController.Play(animData, true);
                        
                        newSkeletonAnimation.MatchRectTransformWithBounds();

                        if (fit)
                            newSkeletonAnimation.rectTransform.FitRectTransformScale(parent);

                        animationController.gameObject.layer = LayerMask.NameToLayer("UI");

                        result.Resolve(animationController);
                    }
                ).Catch(ex => result.Resolve(null));

            return result;
        }

        /**
         * Для отрисовки на карте
         */
        public IPromise<AbstractAnimationController> CreateSpineAnimation(string path, Transform parent = null)
        {
            var result = new Promise<AbstractAnimationController>();
            Loader.Load<SkeletonDataAsset>(path)
                .Then(asset =>
                    {
                        var gameObject = SkeletonAnimation.NewSkeletonAnimationGameObject(asset).gameObject;
                        gameObject.name = path;
                        if (parent)
                        {
                            gameObject.transform.SetParent(parent);
                            gameObject.transform.localPosition = new Vector3(0, 0, 0);
                        }
                        result.Resolve(gameObject.CreateAnimationController());
                    }
                ).Catch(ex => result.Resolve(null));

            return result;
        }
        
        public bool HasAddressable(string path) => Addressables.ResourceLocators.Any(x => x.Keys.Contains(path));
        
        public IPromise<AbstractAnimationController> CreateAnimation(string path, Transform parent = null)
        {
            var result = new Promise<AbstractAnimationController>();
            Create<GameObject>(path, transform)
                .Then(go =>
                {
                    var animationController = go.CreateAnimationController();
                    result.Resolve(animationController);
                })
                .Catch(ex => result.Resolve(null));

            return result;
        }

        public IPromise<T> Create<T>(string path, Transform parent = null, bool needCache = false)
            where T : UnityEngine.Object
        {
            var parentIsNotNull = parent != null;
            var result = new Promise<T>();

            if (path.IsNullOrEmpty() || Loader == null)
            {
                GameLogger.error($"[ERROR] path null: {path.IsNullOrEmpty()} loader null: {Loader == null}");
                result.Resolve(null);
                return result;
            }

			IPromise<T> loadPromise = needCache 
				? Loader.LoadAndCache<T>(path)
				: Loader.Load<T>(path);

			loadPromise.Then(prefab =>
            {
                if (prefab && (parentIsNotNull && parent || !parentIsNotNull))
                {
                    var resultObject = Instantiate(prefab, parent);
                    result.Resolve(resultObject);
                }
                else result.Resolve(null);
            });
            

            return result;
        }
        
        public List<string> GetAssetsPath(string pathPart) => Addressables.ResourceLocators
            .SelectMany(x => x.Keys)
            .OfType<string>()
            .Where(x => x.StartsWith(pathPart))
            .ToList();
    }
}