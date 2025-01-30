using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Animations;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Core.AssetsManager
{
    public class AssetsLoader
    {
        private AssetsManager Manager;
        private Dictionary<string, object> nowLoading = new Dictionary<string, object>();
        private Dictionary<string, List<AsyncOperationHandle>> LoadedAddressablesHandle = new Dictionary<string, List<AsyncOperationHandle>>();
        private List<Object> LoadedResources = new List<Object>();
        private HashSet<object> AddressableKeys;
        private Dictionary<string, string> AddressableWindows = new Dictionary<string, string>();
        public AssetManagerCache Cache = new AssetManagerCache();
        
        public AssetsLoader(AssetsManager manager)
        {
            Manager = manager;
        }

        public Promise LoadAdditionContentCatalog(string[] catalogUrls, Action<float> onProgress = null)
        {
            var result = new Promise();
            if (catalogUrls == null || catalogUrls.Length == 0)
            {
                result.Resolve();
                return result;
            }

            IPromise promises = Promise.Resolved();
            Dictionary<string, float> loadingProgresses = new Dictionary<string, float>();
            
            foreach (var catalog in catalogUrls)
            {
                string curCatalog = catalog;
                promises = promises
                    .Then(() => LoadAdditionContentCatalog(catalog, prc => onCatalogProgress(curCatalog, prc)));
            }

            promises
                .Then(result.Resolve)
                .Catch(ex => result.Reject(ex));

            void onCatalogProgress(string catalog, float prc)
            {
                if (!loadingProgresses.ContainsKey(catalog))
                    loadingProgresses[catalog] = 0f;
                else
                    loadingProgresses[catalog] = prc;

                onProgress?.Invoke(getAllProgress());
            }

            float getAllProgress() => loadingProgresses.Sum(prc => prc.Value / catalogUrls.Length);
            
            return result;
        }
        
        /// <summary>
        /// catalogUrl - относительный путь до каталога  $"{AssetsManager.ResourcesLoadServerPath}/{BuildTarget}/{catalogUrl}";
        /// </summary>
        /// <param name="catalogUrl"></param>
        /// <returns></returns>
        public IPromise LoadAdditionContentCatalog(string catalogUrl, Action<float> onProgress = null, bool isLocal = false)
        {
            var result = new Promise();
            Debug.Log("Start load catalog : " + catalogUrl);
            Manager.StartCoroutine(LoadAdditionContentCatalogCorutine(catalogUrl, result, onProgress, isLocal));
            return result;
        }

        private string BuildTarget
        {
            // AddressableAssets.PlatformMappingService.GetPlatform дает не верный вариант для WSA = WindowsUniversal
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
						#if UNITY_WEBGL
							return "WebGL";
						#endif
						return "Android";
                    case RuntimePlatform.Android: return "Android";
                    case RuntimePlatform.IPhonePlayer: return "iOS";
                    case RuntimePlatform.WebGLPlayer: return "WebGL";
                    case RuntimePlatform.WSAPlayerARM:
                    case RuntimePlatform.WSAPlayerX64:
                    case RuntimePlatform.WSAPlayerX86:
                        return "WSAPlayer";
                    default:
                        throw new ArgumentOutOfRangeException(
                            $"Неизвестная платформа для адрессаблов: {Application.platform}");
                }
            }
        }
        
        private IEnumerator LoadAdditionContentCatalogCorutine(string catalogUrl, Promise promise, Action<float> onProgress = null, bool isLocal = false)
        {
            var absoluteCatalogUrl = 
                !isLocal ? $"{AssetsManager.ResourcesLoadServerPath}/{BuildTarget}/{catalogUrl}"
                         : $"{UnityEngine.AddressableAssets.Addressables.RuntimePath}/{catalogUrl}";
            var handle = Addressables.LoadContentCatalogAsync(absoluteCatalogUrl);
            while (!handle.IsDone)
            {
                onProgress?.Invoke(handle.PercentComplete);
                yield return null;
            }
            
            onProgress?.Invoke(1f);
            
            switch (handle.Status)
            {
                case AsyncOperationStatus.Succeeded:
                    Debug.Log("Catalog loaded : " + absoluteCatalogUrl);
                    promise.Resolve();
                    break;
                case AsyncOperationStatus.Failed:
                    Debug.LogError($"Error catalog loading: {absoluteCatalogUrl} (ex: {handle.OperationException})");
                    promise.Reject(handle.OperationException);

                    //if (BuildSettings.BuildSettings.IsFarmEditor)
                    //    GameReloader.Reload(false);
                    break;
            }
        }

        public AsyncOperationHandle<IResourceLocator> AddressableInit(Action onComplete)
        {
            var addressable = Addressables.InitializeAsync();

            addressable.Completed += onInit;

            return addressable;

            void onInit(AsyncOperationHandle<IResourceLocator> resourceLocator)
            {
                AddressableKeys = new HashSet<object>(resourceLocator.Result.Keys);

                foreach (var obj in AddressableKeys)
                {
                    if (obj is string str)
                    {
                        var path = str;
                        var split = str.Split('/');
                        var name = split.Length > 0 ? split.Last() : str;

                        if (name.Contains("Screen") || name.Contains("Window"))
                        {
                            AddressableWindows[name] = path;
                        }
                    }
                }

                addressable.Completed -= onInit;
                onComplete?.Invoke();
            }
        }

        public IPromise UnloadAll()
        {
            Cache.Clear();
            LoadedResources.Clear();
            
            return Utils.Utils.AsyncOperationToPromise(Resources.UnloadUnusedAssets())
                .Then(GC.Collect);
        }
        
        public IPromise<Dictionary<string, T>> Load<T>(List<string> paths, Action<float> onProgress)
            where T : Object
        {
            var result = new Promise<Dictionary<string, T>>();
            var resultPrefabs = new Dictionary<string, T>();
            var percentPerOne = 1f / paths.Count;
            var curProgress = 0f;
            var allLoadPromises = new List<IPromise<(string, T)>>();

            foreach (var path in paths)
            {
                var curPromise = LoadWithName<T>(path);
                curPromise.Then(
                    res =>
                    {
                        resultPrefabs[res.path] = res.prefab;
                        curProgress += percentPerOne;
                        onProgress?.Invoke(curProgress);
                    }
                );
                
                allLoadPromises.Add(curPromise);
            }

            Promise<(string path, T prefab)>.All(allLoadPromises)
                .Then(
                    allLoadedObjects =>
                    {
                        onProgress?.Invoke(1f);
                        result.Resolve(resultPrefabs);
                    });

            return result;
        }

        public void UnloadAddressable(List<string> paths) => paths.ForEach(UnloadAddressable);

        public void UnloadAddressable(string path)
        {
            //Обязательно удаляем из кэша
            Cache.Remove(path);
            
            if (LoadedAddressablesHandle.ContainsKey(path))
            {
                LoadedAddressablesHandle[path].ForEach(Addressables.Release);
                LoadedAddressablesHandle.Remove(path);
            }
        }

        public IPromise<T> LoadAndCache<T>(string path, Action<float> onProgress = null) where T : Object
        {
            return Load<T>(path, onProgress).Then(p =>
            {
                Cache[path] = p;
                return p;
            });
        }

        public IPromise<Dictionary<string, T>> LoadAndCache<T>(List<string> paths, Action<float> onProgress = null) where T : Object
        {
            return Load<T>(paths, onProgress).Then(Cache.Add);
        }

        public IPromise<(string path, T prefab)> LoadWithName<T>(string path) where T : Object
        {
            return Load<T>(path).Then(p => Promise<(string, T)>.Resolved((path, p)));
        }

        public T LoadSync<T>(string path) where T : Object
        {
            return Resources.Load<T>(path);
        }

        public IPromise<T> Load<T>(string path, Action<float> onProgress = null)
            where T : Object
        {
            //Если уже грузится этот ресурс, то возвращаем его
            if (nowLoading.ContainsKey(path) && nowLoading[path] is Promise<T> res)
                return res;
            
            var result = new Promise<T>();

            //Пробуем взять из кэша
            if (Cache.ContainsKey(path))
            {
                result.Resolve(Cache[path] as T);
                return result;
            }
            
            nowLoading[path] = result;
            result.Finally(() => nowLoading.Remove(path));

            var firstLoad = new Promise<T>();
            var addressablesFirst = Addressables.ResourceLocators.Any(x => x.Locate(path, typeof(T), out var _));

            Game.Instance.StartCoroutine(!addressablesFirst
                ? LoadFromResources(path, firstLoad, onProgress)
                : LoadFromAddressables(path, firstLoad, onProgress));

            firstLoad.Then(x =>
            {
                result.Resolve(x ? x : null);
            });

            return result;
        }

        private IEnumerator LoadFromResources<T>(string path, Promise<T> promise, Action<float> onProgress) where T : Object
        {
            if (AssetsManager.NeedLog)
                GameLogger.debug($"Trying get {path} from resources");
            ResourceRequest request = Resources.LoadAsync<T>(path);
            
            while (!request.isDone)
            {
                onProgress?.Invoke(request.progress);
                yield return null;
            }
                
            onProgress?.Invoke(1f);

            if (request.asset != null)
            {
                if (!LoadedResources.Contains(request.asset))
                    LoadedResources.Add(request.asset);
                    
                if (AssetsManager.NeedLog)
                    Debug.Log($"{path} loaded from resources");
                promise.Resolve(request.asset as T);
            }
            else
            {
                if (AssetsManager.NeedLog)
                    Debug.Log($"{path} not loaded from resources");
                promise.Resolve(null);
            }
        }
        
        private void FixSpritesShaders<T>(T prefab) where T : Object
        {
            if (typeof(T) == typeof(GameObject))
            {
                var gameObject = prefab as GameObject;

                var allRenderers = gameObject.GetComponentsInChildren<SpriteRenderer>(true);
                foreach (var renderer in allRenderers)
                    renderer.sharedMaterial.shader = Shader.Find(renderer.sharedMaterial.shader.name);

                var allTileMaps = gameObject.GetComponentsInChildren<TilemapRenderer>(true);
                foreach (var renderer in allTileMaps)
                    renderer.sharedMaterial.shader = Shader.Find(renderer.sharedMaterial.shader.name);

                var allParticles = gameObject.GetComponentsInChildren<ParticleSystemRenderer>(true);
                foreach (var particle in allParticles)
                    particle.sharedMaterial.shader = Shader.Find(particle.sharedMaterial.shader.name);

                var allSpine = gameObject.GetComponentsInChildren<SkeletonAnimation>(true);
                foreach (var spine in allSpine)
                {
                    SpineSkeletonAnimationController.ChangeShader(spine);
                }
            }
        }

        private IEnumerator LoadFromAddressables<T>(string path, Promise<T> promise, Action<float> onProgress, int retryCount = 1) where T : Object
        {
            if (AssetsManager.NeedLog) Debug.Log($"Trying get {path} from addressables");
            var request = Addressables.LoadAssetAsync<T>(path);

            while (!request.IsDone)
            {
                onProgress?.Invoke(request.PercentComplete);
                yield return null;
            }

            onProgress?.Invoke(1f);

            if (request.Status == AsyncOperationStatus.Succeeded)
            {
                
#if UNITY_EDITOR
                FixSpritesShaders(request.Result);
#endif
                
                if(!LoadedAddressablesHandle.ContainsKey(path))
                    LoadedAddressablesHandle[path] = new List<AsyncOperationHandle>();
                LoadedAddressablesHandle[path].Add(request);

                if (AssetsManager.NeedLog) Debug.Log($"{path} loaded from addressables");
                promise.Resolve(request.Result);
            }
            else
            {
                if (retryCount > 0)
                {
                    if (AssetsManager.NeedLog) Debug.Log($"Retrying to get {path} from addressables");
                    Game.Instance.StartCoroutine(LoadFromAddressables(path, promise, onProgress, retryCount - 1));
                }
                else
                {
                    Debug.LogError($"Error while loading Addressable {path}");
                    promise.Resolve(null);
                }
            }
        }
        
        public IPromise<Texture2D> LoadURLImage(string url)
        {
            var result = new Promise<Texture2D>();
            Game.Instance.StartCoroutine(DownloadImage(url, result));
            return result;
        }
        
        private IEnumerator DownloadImage(string url, Promise<Texture2D> promise)
        {   
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (request.isNetworkError || request.isHttpError)
            {
                promise.Reject(new Exception($"Error while loading URL {url}"));
            }
            else
            {
                promise.Resolve(((DownloadHandlerTexture) request.downloadHandler).texture);
            }
        }

        public string TryGetPathByType(Type abstractWindow)
        {
            if (AddressableWindows.ContainsKey(abstractWindow.Name))
                return AddressableWindows[abstractWindow.Name];

            return null;
        }
    }
}