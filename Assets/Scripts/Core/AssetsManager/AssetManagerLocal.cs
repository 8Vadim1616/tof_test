using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts.Core.AssetsManager
{
    /// <summary>
    /// Выгружает загруженные через него ресурсы при удалении.
    /// </summary>
    public class AssetManagerLocal
    {
        private List<string> _paths = new List<string>();

        public IPromise<T> Create<T>(string path, Transform parent = null) where T : Object
        {
            var result = AssetsManager.Instance.Create<T>(path, parent);
            result.Then(_ => AddPath(path));
            return result;
        }
        
        public IPromise<T> Load<T>(string path, Action<float> onProgress = null) where T : Object
        {
            var result = AssetsManager.Instance.Loader.Load<T>(path, onProgress);
            result.Then(_ => AddPath(path));
            return result;
        }

        public IPromise<Dictionary<string, T>> Load<T>(List<string> paths, Action<float> onProgress)
            where T : Object
        {
            var result = AssetsManager.Instance.Loader.Load<T>(paths, onProgress);
            result.Then(_ => AddPath(paths));
            return result;
        }
        
        public IPromise<T> LoadAndCache<T>(string path, Action<float> onProgress = null) where T : Object
        {
            var result = AssetsManager.Instance.Loader.LoadAndCache<T>(path, onProgress);
            result.Then(_ => AddPath(path));
            return result;
        }

        public IPromise<Dictionary<string, T>> LoadAndCache<T>(List<string> paths, Action<float> onProgress = null) where T : Object
        {
            var result = AssetsManager.Instance.Loader.LoadAndCache<T>(paths, onProgress);
            result.Then(_ => AddPath(paths));
            return result;
        }
        
        private void AddPath(List<string> paths) => paths.ForEach(_paths.AddOnce);
        private void AddPath(string path) => _paths.AddOnce(path);
        
        public void Unload()
        {
            AssetsManager.Instance.Loader.UnloadAddressable(_paths);
            _paths.Clear();
        }
    }
}