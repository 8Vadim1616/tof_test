using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Assets.Scripts.Static;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using SimpleDiskUtils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Network
{
	public class GroupProbability : StaticCollectionItem
	{
		[JsonProperty("ver")]
		public int Probability { get; private set; }
	}
	public class GroupsProbability : Dictionary<string, GroupProbability>
	{
		public const string NAME = "reg_groups";

		public int GetRandomGroup()
		{
			var maxProbs = Values.Sum(g => g.Probability);
			var randProb = Random.Range(0f, maxProbs);
			var curProb = 0;

			foreach (var value in Values)
			{
				curProb += value.Probability;
				if (curProb > randProb)
					return value.Id;
			}

			return Values.LastOrDefault()?.Id ?? 1;
		}
	}

	public static class FileResourcesLoader
	{
		public const string CRC_FILE = "files_crc";
		public const string MODEL_DATA_FILE = "model_data";

		private static readonly Dictionary<long, FileVersionsByGroup> _groups = new Dictionary<long, FileVersionsByGroup>();

		public static FileVersionsByGroup GetGroup(long grp)
		{
			if (!_groups.ContainsKey(grp))
				_groups[grp] = new FileVersionsByGroup(grp);
			return _groups[grp];
		}

		public static FileVersionsByGroup NoGroup() => GetGroup(0);

		public static List<long> AvailableGroups => NoGroup()
			.LoadJson<GroupsProbability>(GroupsProbability.NAME)
			.Select(x => (long) x.Value.Id)
			.ToList();

		public static bool HasResources(long grp) => grp == 0 || AvailableGroups.Contains(grp);

		private static Dictionary<string, object> writeLockers = new Dictionary<string, object>();
		public static bool WriteTextToFile(string filename, string text)
		{
			if (!writeLockers.ContainsKey(filename))
				writeLockers[filename] = new object();
			
			lock (writeLockers[filename])
			{
				try
				{
					if (!Utils.Utils.CheckAvailableSpace(text))
						return false;
					
					using (var streamWriter = new StreamWriter(filename, append: false, encoding: Encoding.UTF8, bufferSize: 1048576))
					{
						streamWriter.WriteLine(text);
						return true;
					}
				}
				catch (Exception e)
				{
					GameLogger.error($"[WriteTextToFile] Save file '{filename}' err: {e}");
					return false;
				}
			}
		}

		public static IList<int> GetSavedGroupsNums()
		{
			var groupsNums = new List<int>();

			var directories = new DirectoryInfo(Application.persistentDataPath).GetDirectories();
			foreach (var directory in directories)
				if (directory.Name.StartsWith("grp_") && int.TryParse(directory.Name.Substring(4), out int groupNum))
				{
					Debug.Log($"Adding group {groupNum}");
					groupsNums.Add(groupNum);
				}

			return groupsNums;
		}

		public static long GetUnityLogsFolderSize()
		{
			try
			{
				long groupSize = 0L;
				var files = new DirectoryInfo($"{Application.persistentDataPath}/Logs/").GetFiles();
				foreach (var file in files)
					groupSize += file.Length;

				return groupSize;
			}
			catch
			{
				return -1;
			}
		}

		public static void ClearAllGroups()
		{
			Debug.Log("[FileResourcesLoader] Clearing all groups.");

			var directories = new DirectoryInfo(Application.persistentDataPath).GetDirectories();
			foreach (var directory in directories)
				if (directory.Name.StartsWith("grp_"))
				{
					directory.Delete(true);
					Debug.Log($"[FileResourcesLoader] Cleared group {directory.Name}.");
				}
		}
	}

	public class FileVersionsByGroup
	{
		private const string TAG = "[FileResourcesLoader] ";

		private readonly long _group;
		private Dictionary<string, long> _fileVersions;
		private Dictionary<string, long> _resourceVersions;

		public FileVersionsByGroup(long group)
		{
			_group = group;
			_resourceVersions = LoadJsonFromResources<Dictionary<string, long>>(FileResourcesLoader.CRC_FILE) ?? new Dictionary<string, long>();
			_fileVersions = LoadJsonFromFile<Dictionary<string, long>>(FileResourcesLoader.CRC_FILE) ?? new Dictionary<string, long>();
		}

		public long GetMax(string fileName) => Math.Max(GetFromFiles(fileName), GetFromResources(fileName));
		public long GetFromFiles(string fileName) => _fileVersions.TryGetValue(fileName, out var ver) ? ver : default;

		public long GetFromResources(string fileName) => _resourceVersions.TryGetValue(fileName, out var ver) ? ver : default;

		/// <summary>
		/// Загрузить json файл из файла. Смотрим на версию в ресурсах и версию в persistentDataPath. Где версия больше,
		/// оттуда и грузим
		/// </summary>
		/// <param name="fileName">Имя файла БЕЗ расширения</param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public T LoadJson<T>(string fileName)
        {
			var versionFromResources = GetFromResources(fileName);
			var versionFromFiles = GetFromFiles(fileName);

			if (versionFromFiles > versionFromResources)
			{
				try
				{
					return LoadJsonFromFile<T>(fileName, false);
				}
				catch (Exception e)
				{
					_fileVersions.Remove(fileName);
					SaveJson(FileResourcesLoader.CRC_FILE, _fileVersions);

					Debug.LogError(e);
				}
			}
			return LoadJsonFromResources<T>(fileName);
		}

		public IEnumerable<string> LoadLines(string fileName)
		{
			var versionFromResources = GetFromResources(fileName);
			var versionFromFiles = GetFromFiles(fileName);

			if (versionFromFiles > versionFromResources)
			{
				try
				{
					return LoadTextLinesFromFile(fileName);
				}
				catch (Exception e)
				{
					_fileVersions.Remove(fileName);
					SaveJson(FileResourcesLoader.CRC_FILE, _fileVersions);

					Debug.LogError(e);
				}
			}
			return LoadLinesFromResources(fileName);
		}
		
		public string LoadText(string fileName)
		{
			var versionFromResources = GetFromResources(fileName);
			var versionFromFiles = GetFromFiles(fileName);

			if (versionFromFiles > versionFromResources)
			{
				try
				{
					return LoadTextFromFile(fileName);
				}
				catch (Exception e)
				{
					_fileVersions.Remove(fileName);
					SaveJson(FileResourcesLoader.CRC_FILE, _fileVersions);

					Debug.LogError(e);
				}
			}
			return LoadTextFromResources(fileName);
		}

		public string GetFilesGroupPath() => _group > 0 ? $"{Application.persistentDataPath}/grp_{_group}/" : Application.persistentDataPath + "/";
		public string GetResourcesGroupPath() => _group > 0 ? $"Static/grp_{_group}/" : "Static/";
		public string GetJsonPath(string filename) => GetFilesGroupPath() + filename + ".json";
		public string GetResourcesPath(string filename) => GetResourcesGroupPath() + filename;
		public bool IsJsonExist(string filename) => File.Exists(GetJsonPath(filename));

		/// <summary>
		/// Записать в json файл.
		/// </summary>
		/// <param name="fileName">Имя файла БЕЗ расширения</param>
		/// <param name="data">Строка данных</param>
		public bool SaveJsonString(string fileName, string data)
		{
			var dir = new DirectoryInfo(GetFilesGroupPath());
			if (!dir.Exists)
				dir.Create();

			var path = GetJsonPath(fileName);
			return FileResourcesLoader.WriteTextToFile(path, data);
		}

		/// <summary>
		/// Записать в json файл.
		/// </summary>
		/// <param name="fileName">Имя файла БЕЗ расширения</param>
		/// <param name="data">Объект данных</param>
		public bool SaveJson(string fileName, object data)
		{
			Formatting formatting;
#if UNITY_EDITOR
			formatting = Formatting.Indented;
#else
			formatting = Formatting.None;
#endif
			return SaveJsonString(fileName, JsonConvert.SerializeObject(data, formatting));
		}

		/// <summary>
		/// Записать в json файл.
		/// </summary>
		/// <param name="fileName">Имя файла БЕЗ расширения</param>
		/// <param name="data">Объект данных</param>
		/// <param name="jsonString">Записываемая строка</param>
		public bool SaveJson(string fileName, object data, out string jsonString)
		{
			Formatting formatting;
#if UNITY_EDITOR
			formatting = Formatting.Indented;
#else
			formatting = Formatting.None;
#endif
			jsonString = JsonConvert.SerializeObject(data, formatting);

			return SaveJsonString(fileName, jsonString);
		}


		public void RemoveJson(string fileName, bool needError = true)
		{
			try
			{
				File.Delete(GetJsonPath(fileName));
			}
			catch (Exception e)
			{
				if (needError)
					Debug.LogError("Can't remove " + GetJsonPath(fileName) + "\n" + e);
			}
		}

        /// <summary>
        /// Возвращает максимальную версию файла, которая есть на клиенте
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public long GetFileVersion(string fileName)
        {
            return Math.Max(GetVersionFromFiles(fileName), GetVersionFromResources(fileName));
        }

        /// <summary>
        /// Сохранить json с версией
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="version"></param>
        public void SaveFileWithVersion(string fileName, object data, long version)
        {
			var wasFileVersions = new Dictionary<string, long>(_fileVersions);
			try
			{
				_fileVersions[fileName] = version;
				if (data is string stringData)
				{
					if (SaveJsonString(fileName, stringData))
						SaveJson(FileResourcesLoader.CRC_FILE, _fileVersions);
				}
				else
				{
					if (SaveJson(fileName, data))
						SaveJson(FileResourcesLoader.CRC_FILE, _fileVersions);
				}
			}
			catch (Exception e)
			{
				_fileVersions = wasFileVersions;
				SaveJson(FileResourcesLoader.CRC_FILE, _fileVersions);
				throw e;
			}
        }
		
		private IEnumerable<string> LoadLinesFromResources(string fileName)
		{
			Debug.Log(TAG + $"Loading lines {fileName} from resources");

			//грузим ресурсы из стандартной группы, если такой группы нет в билде
			if (_group != StaticData.DEFAULT_GRP && !FileResourcesLoader.HasResources(_group))
				return FileResourcesLoader.GetGroup(StaticData.DEFAULT_GRP).LoadLinesFromResources(fileName);

			var fileInResources = Resources.Load<TextAsset>(GetResourcesPath(fileName));
			if (fileInResources && !fileInResources.text.IsNullOrEmpty())
			{
				var text = fileInResources.text;
				List<string> strContent = new List<string>();

				using (var reader = new StringReader(text))
				{
					while (true)
					{
						var line = reader.ReadLine();
						if (line == null)
							break;
						strContent.Add(line);
					}
				}

				return strContent;
			}

			return default;
		}

		private string LoadTextFromResources(string fileName)
		{
			Debug.Log(TAG + $"Loading {fileName} from resources");

			//грузим ресурсы из стандартной группы, если такой группы нет в билде
			if (_group != StaticData.DEFAULT_GRP && !FileResourcesLoader.HasResources(_group))
				return FileResourcesLoader.GetGroup(StaticData.DEFAULT_GRP).LoadTextFromResources(fileName);

			var fileInResources = Resources.Load<TextAsset>(GetResourcesPath(fileName));
			return fileInResources ? fileInResources.text : default;
		}

		private string[] LoadTextLinesFromFile(string fileName)
		{
			Debug.Log(TAG + $"Loading lines {fileName}.json from files");
			var path = GetJsonPath(fileName);
			if (!File.Exists(path))
			{
				Debug.LogWarning($"File lines {fileName}.json doesn't exist");
				return default;
			}

			var file = File.ReadAllLines(path);

			if (file.IsNullOrEmpty())
			{
				Debug.LogWarning($"[LoadTextLinesFromFile] File {fileName}.json exist but empty");
				return default;
			}

			return file;
		}
		
		private string LoadTextFromFile(string fileName)
		{
			Debug.Log(TAG + $"Loading {fileName}.json from files");
			var path = GetJsonPath(fileName);
			if (!File.Exists(path))
			{
				Debug.LogWarning($"File {fileName}.json doesn't exist");
				return default;
			}

			var file = File.ReadAllText(path);

			if (string.IsNullOrEmpty(file))
			{
				Debug.LogWarning($"[LoadJsonFromFile] File {fileName}.json exist but empty");
				return default;
			}

			return file;
		}

		private T LoadJsonFromResources<T>(string fileName)
        {
			//грузим ресурсы из стандартной группы, если такой группы нет в билде
			if (_group != StaticData.DEFAULT_GRP && !FileResourcesLoader.HasResources(_group))
				return FileResourcesLoader.GetGroup(StaticData.DEFAULT_GRP).LoadJsonFromResources<T>(fileName);

			var fileInResources = Resources.Load<TextAsset>(GetResourcesPath(fileName));
			if (!fileInResources)
				return default;

			if (!string.IsNullOrEmpty(fileInResources.text))
			{
				try
				{
					return JsonConvert.DeserializeObject<T>(fileInResources.text);
				}
				catch (Exception e)
				{
					GameLogger.error($"{TAG}Bad file {GetResourcesPath(fileName)}\n{e.Message}");
					return default;
				}
			}

			return default;
		}

        public T LoadJsonFromFile<T>(string fileName, bool safely = true)
        {
			try
			{
				var path = GetJsonPath(fileName);
				var file = File.ReadAllText(path);
				return JsonConvert.DeserializeObject<T>(file);
			}
			catch (Exception e)
			{
				if (safely)
					return default;
				throw e;
			}
		}

		public long GetGroupSize()
		{
			long groupSize = 0L;
			var files = new DirectoryInfo(GetFilesGroupPath()).GetFiles();
			foreach (var file in files)
				groupSize += file.Length;

			return groupSize;
		}

		public long GetFileSize(string fileName, bool safely = true)
		{
			try
			{
				var path = GetJsonPath(fileName);
				var fileInfo = new FileInfo(path);
				return fileInfo.Length;
			}
			catch (Exception e)
			{
				if (safely)
					return -1L;
				throw e;
			}
		}

		private void LoadVersionsFromFiles()
        {
            if (_fileVersions != null)
                return;

			_fileVersions = LoadJsonFromFile<Dictionary<string, long>>(FileResourcesLoader.CRC_FILE)
                                  ?? new Dictionary<string, long>();
        }
        
        private void LoadVersionsFromResources()
        {
            if (_resourceVersions != null)
                return;

			_resourceVersions = LoadJsonFromResources<Dictionary<string, long>>(FileResourcesLoader.CRC_FILE)
                                  ?? new Dictionary<string, long>();
        }

        private long GetVersionFromResources(string fileName)
        {
            LoadVersionsFromResources();
            return _resourceVersions.ContainsKey(fileName) ? _resourceVersions[fileName] : 0;
        }
        
        private long GetVersionFromFiles(string fileName)
        {
            LoadVersionsFromFiles();
            return _fileVersions.ContainsKey(fileName) ? _fileVersions[fileName] : 0;
        }

        public Dictionary<string, long> GetMaxFilesVersion()
        {
            var versionsFromResources = _resourceVersions;
            var versionsInFiles = _fileVersions;
            var allFiles = versionsFromResources.Keys.Union(versionsInFiles.Keys);
            var result = new Dictionary<string, long>();
            
            foreach (var fileName in allFiles)
            {
                var fileVersionFromResources =
                    versionsFromResources.ContainsKey(fileName) ? versionsFromResources[fileName] : 0;
                var fileVersionFromFiles =
                    versionsInFiles.ContainsKey(fileName) ? versionsInFiles[fileName] : 0;
                result[fileName] = Math.Max(fileVersionFromResources, fileVersionFromFiles);
            }

            return result;
        }
    }
}