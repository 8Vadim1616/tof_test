using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Queries;
using Assets.Scripts.Network.Queries.Operations.Api.StaticData;
using Assets.Scripts.Static.UserGroups;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.UI.WindowsSystem.Editor
{
    /// <summary>
    /// Загружает с сервера все статичные файлы (модель) и сохраняет её в ресурсах
    /// </summary>
    public class EditorStaticDataUpdater
    {
        private const string TAG = "[EditorStaticDataUpdater]: ";
        private const string STATIC_PATH = "Assets/Resources/Static/";

		[MenuItem("Build/Update Static Data", false, 0)]
		public static IPromise UpdateStaticData()
		{
			return UpdateGroups()
				.Then(UpdateAllGroupsStaticData);
			//LoadData(StaticDataFileName.GetAllForBuild());
		}

		private static IPromise UpdateStaticData(long group)
		{
			var parameters = new JObject
			{
				["ver"] = Application.version,
				["model_grp"] = group,
				["files"] = new JArray(),
				["build"] = GameConsts.BuildNameForModel
			};

			foreach (var file in StaticDataFileName.GetAllForBuild())
				(parameters["files"] as JArray).Add(new JObject { ["name"] = file });

			Debug.Log(JsonConvert.SerializeObject(parameters));

			return Request(GetRequest("get_model_data.php"), parameters)
				.Then(jsonStringAnswer =>
				{
					var response = UpdateStaticDataOperation.StaticDataResponse.GetFromRawResponse(jsonStringAnswer);
					SaveAllFiles(response, group);

					Debug.Log(TAG + $"Static from group {group} saved");

					return Promise.Resolved();
				});
		}

		private static IPromise UpdateAllGroupsStaticData()
		{
			ClearAllGroupsFolders();

			var promise = Promise.Resolved();

			Debug.Log(TAG + $"Start get static data from groups: {FileResourcesLoader.AvailableGroups.Join(", ", grp => grp.ToString())}");
			foreach (var group in FileResourcesLoader.AvailableGroups)
			{
				if (group == 99)
					continue;

				var grp = group;
				promise = promise.Then(() => UpdateStaticData(grp));
			}

			return promise;
		}

		private static void ClearAllGroupsFolders()
		{
			var directory = new DirectoryInfo(STATIC_PATH);
			foreach (var grpDirectory in directory.GetDirectories())
				if (grpDirectory.Name.StartsWith("grp_"))
				{
					foreach (var file in grpDirectory.GetFiles())
						file.Delete();
					grpDirectory.Delete();
					var metaDirectoryFile = new FileInfo(grpDirectory.FullName + ".meta");
					if (metaDirectoryFile.Exists)
						metaDirectoryFile.Delete();
				}
		}

		private static IPromise UpdateGroups()
		{
			var requestFiles = new[]
			{
				GroupsProbability.NAME,
				StaticUserGroups.FILE_NAME
			};

			var requestBody = new JObject
			{
				["build"] = GameConsts.BuildNameForModel,
				["ver"] = Application.version,
				["model_grp"] = 1,
				["files"] = new JArray
				{
					requestFiles.Select(x => new JObject {["name"] = x } ),
				}
			};
			
			Debug.Log(JsonConvert.SerializeObject(requestBody));

			return Request(GetRequest("get_model_data.php"), requestBody)
				.Then(jsonStringAnswer =>
				{
					var response = UpdateStaticDataOperation.StaticDataResponse.GetFromRawResponse(jsonStringAnswer);

					response.Files = response.Files					// todo временный костыль, куча мусора в ответе
						.Where(x => requestFiles.Contains(x.Key))
						.ToDictionary(x => x.Key, x => x.Value);

					SaveAllFiles(response, 0);
					Debug.Log(TAG + "Groups data saved");
					return Promise.Resolved();
				});
		}

		private static IPromise<string> Request(Uri uri, JObject parameters)
		{
			var promise = new Promise<string>();

			var jsonRequestString = JsonConvert.SerializeObject(parameters);
			var signature = Scripts.Utils.Utils.Hash(jsonRequestString + QueryManager.SALT);

			var request = new UnityWebRequest
			{
				url = uri.ToString(),
				method = UnityWebRequest.kHttpVerbPOST,
				downloadHandler = new DownloadHandlerBuffer(),
				uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(signature + jsonRequestString))
				{
					contentType = "application/json"
				},
				disposeUploadHandlerOnDispose = true,
				disposeDownloadHandlerOnDispose = true,
			};

			request.SendWebRequest().completed += operation =>
			{
				promise.Resolve(request.downloadHandler.text);
				request.Dispose();
			};

			return promise;
		}

		private static void SaveAllFiles(UpdateStaticDataOperation.StaticDataResponse response, long group)
		{
			var crc = new Dictionary<string, long>();

			foreach (var kv in response.Files)
				if (kv.Value.Data != null)
				{
					SaveStaticData(kv.Key, kv.Value.Data, group);
					crc[kv.Key] = kv.Value.Ver.Value;
				}

			SaveStaticData(FileResourcesLoader.CRC_FILE, crc, group);
			SaveStaticData(FileResourcesLoader.MODEL_DATA_FILE, response.ModelData, group);

			AssetDatabase.Refresh();
		}

		private static void SaveStaticData(string name, object data, long group)
		{
			if (group > 0)
				name = $"grp_{group}/{name}";

			if (!(data is string))
				data = JsonConvert.SerializeObject(data, Formatting.None);
			WriteFile(STATIC_PATH + name + ".json", data);
		}

		public static void WriteFile(string path, object data)
        {
			var file = new FileInfo(path);
			if (file.Directory != null && !file.Directory.Exists)
				file.Directory.Create();

			using (var fs = new FileStream(path, FileMode.Create))
			{
				using (var writer = new StreamWriter(fs))
					writer.Write(data);
			}
		}

        static Uri GetRequest(string requestFile)
        {
            return new Uri(GameConsts.UpdateStaticEntryPoint + requestFile);
        }
    }
}