﻿using UnityEngine;

namespace Assets.Scripts.Utils
{
    /// <summary>
    /// Be aware this will not prevent a non singleton constructor
    ///   such as `T myT = new T();`
    /// To prevent that, add `protected T () {}` to your singleton class.
    /// 
    /// As a note, this is made as MonoBehaviour because we need Coroutines.
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        private static object _lock = new object();

        public static T Create()
        {
            lock (_lock)
            {
                if (_instance != null) return _instance;
                
                _instance = (T)FindObjectOfType(typeof(T));

                if (FindObjectsOfType(typeof(T)).Length > 1)
                {
                    Debug.LogError("Something went really wrong " +
                                     " - there should never be more than 1 singleton!" +
                                     " Reopening the scene might fix it.");
                    return _instance;
                }

                if (_instance == null)
                {
                    GameObject singleton = new GameObject();
                    _instance = singleton.AddComponent<T>();
                    _instance.SendMessage("PostAwake", SendMessageOptions.DontRequireReceiver);
                    singleton.name = "(singleton) " + typeof(T).ToString();

                    DontDestroyOnLoad(singleton);

                    Debug.Log("An instance of " + typeof(T) +
                                     " is needed in the scene, so '" + singleton +
                                     "' was created with DontDestroyOnLoad.");
                }
                else
                {
                    Debug.Log("Using instance already created: " +
                              _instance.gameObject.name);
                }

                return _instance;
            }
        }

        public static T Instance => _instance;
    }
}