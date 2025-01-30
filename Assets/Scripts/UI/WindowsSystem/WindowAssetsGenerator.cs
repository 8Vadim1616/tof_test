#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.UI.WindowsSystem
{
    public static class WindowAssetsGenerator
    {
        [MenuItem("Tools/Generate Window Assets")]
        private static void GenerateWindowsFile() => Generate();

        public static T GenerateAndGetPrefab<T>() where T : AbstractWindow
        {
            Type type = typeof(T);
            T window = (T)Generate().FirstOrDefault(x => x.GetType() == type);
            WindowsHolder.Windows[type] = GetResourcePath(window);
            return window;
        }

        public static AbstractWindow[] Generate()
        {
            Debug.LogWarning($"<color=cyan>{nameof(WindowAssetsGenerator)}.{nameof(Generate)}></color> " +
                             $"Генерирую файл {FILE_PATH}.");

            AbstractWindow[] windows = Resources.LoadAll<AbstractWindow>("");

            string content = GenerateContent(windows);
            SaveFile(content);

            return windows;
        }

        private static string GenerateContent(AbstractWindow[] windows)
        {
            Debug.LogWarning($"<color=cyan>{nameof(WindowAssetsGenerator)}.{nameof(GenerateContent)}></color> " +
                             $"Обрабатываю {windows.Length} окон.");

            string text = "";
            List<Type> added = new List<Type>();
            List<Type> ignored = new List<Type>();
            string ignoredText = "";
            List<Type> duplicated = new List<Type>();
            string duplicatedText = "";

            foreach (AbstractWindow window in windows)
            {
                Type type = window.GetType();
                if (window.IsOnlyOnePrefab)
                {
                    if (added.Contains(type))
                    {
                        duplicated.Add(type);
                        duplicatedText += $"{type}\n";
                    }
                    else
                    {
                        added.Add(type);
                    }

                    string path = GetResourcePath(window);
                    text += $"            [typeof({type})] = \"{path}\",\n";
                }
                else
                {
                    ignored.Add(type);
                    ignoredText += $"{type}\n";
                }
            }

            Debug.LogWarning($"<color=cyan>{nameof(WindowAssetsGenerator)}.{nameof(GenerateContent)}></color> " +
                             $"Добавлено {added.Count} окон");

            if (ignored.Count > 0)
                Debug.LogWarning($"<color=cyan>{nameof(WindowAssetsGenerator)}.{nameof(GenerateContent)}></color> " +
                                 $"Проигнорировано {ignored.Count} окон:\n{ignoredText}");

            if (duplicated.Count > 0)
                Debug.LogWarning($"<color=red>{nameof(WindowAssetsGenerator)}.{nameof(GenerateContent)}></color> " +
                                 $"Обнаружено {duplicated.Count} окон, имеющих более одного префаба:\n{duplicatedText}");


            string content = FILE_TEMPLATE.Replace("//insert//", text);
            return content;
        }

        private static void SaveFile(string content)
        {
            try
            {
                using StreamWriter stream = new StreamWriter(FILE_PATH, append: false);
                stream.Write(content);
            }
            catch (Exception e)
            {
                //Debug.LogError($"Не удалось создать файл с данными {FILE_PATH}. {e.Message}");
            }
        }

        private static string GetResourcePath(AbstractWindow window)
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(window);
            path = CutResourcesFolder(path);
            path = CutPrefabExtension(path);
            return path;
        }

        private static string CutPrefabExtension(string path)
        {
            const string prefab = ".prefab";
            int indexOf = path.LastIndexOf(prefab, StringComparison.Ordinal);
            if (indexOf >= 0)
                path = path.Substring(0, indexOf);
            return path;
        }

        private static string CutResourcesFolder(string path)
        {
            const string res = "Resources/";
            int indexOf = path.IndexOf(res, StringComparison.Ordinal);
            if (indexOf >= 0)
                path = path.Substring(indexOf + res.Length);
            return path;
        }

        private const string FILE_TEMPLATE = @"
using System;
using System.Collections.Generic;

namespace Assets.Scripts.UI.WindowsSystem
{
    // --- файл сгенерирован автоматичеcки --- //
    public static class WindowsHolder
    {
        public static readonly Dictionary<Type, string> Windows = new Dictionary<Type, string>
        {
//insert//            
        };
    }
}";

        private const string FILE_PATH = "Assets/Scripts/UI/WindowsSystem/WindowsHolder.cs";
    }
}
#endif
