using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Core;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ExternalScripts
{
	public class ExternalInterface : Singleton<ExternalInterface>
	{
		private static Dictionary<string, Func<object, object>> Callbacks = new Dictionary<string, Func<object, object>>();

		public static bool IsAvailable => true;
		
		/**
		 * Вызывается из iframe
		 * unityInstance.SendMessage("ExternalInterface", "Callback", '{"name" : "testFunc", "data": 1, "needResponse": false}');
		 * "ExternalInterface" - Название объекта на сцене.
		 * "Callback" - Нзвание функции которую вызвать. Оставить так.
		 * data - JSON что передать; name - название функции; needResponse - по завершению можно вызвать функцию в айфрейме
		 * Предварительно нужно добавить callback ExternalInterface.AddCallback("testFunc", testFunction);
		 */
		public void Callback(object obj)
		{
			var callbackObject = JsonConvert.DeserializeObject<JObject>((string)obj);

			string name = callbackObject["name"].ToString();
			object data = callbackObject["data"];

			bool needResponse = false;

			if (callbackObject.TryGetValue("needResponse", out JToken val))
				needResponse = val.ToObject<bool>();

			GameLogger.info("callback " + obj);
			
			if (Callbacks.ContainsKey(name))
			{
				var result = Callbacks[name](data);

				if (result != null)
				{
					if (needResponse)
					{
						CallFromIframe("Response_" + name, data);
					}
				}
			}
		}
		
		/**Добавить callback с названием name, для доступа из iframe**/
		public static void AddCallback(string name, Func<object, object> callback)
		{
			if (Callbacks.ContainsKey(name))
				Callbacks.Remove(name);

			Callbacks.Add(name, callback);
		}
		
		/**Вызвать функцию из iframe**/
		public static void CallFromIframe(string functionName, params object[] args)
		{
			if (!IsAvailable)
				return;

			Application.ExternalCall(functionName, args);
		}

		/**Вывести лог в консоль браузера**/
		public static void Info(string str)
		{
			if (!IsAvailable)
				return;

			Application.ExternalCall("console.log", str);
		}
		
		/**Вывести варнинг в консоль браузера**/
		public static void Warning(string str)
		{
			if (!IsAvailable)
				return;

			Application.ExternalCall("console.warn", str);
		}
		
		/**Вывести эррор в консоль браузера**/
		public static void Error(string str)
		{
			if (!IsAvailable)
				return;

			Application.ExternalCall("console.error", str);
		}

		public static void OnGameLoaded(Func<object, object> callback)
		{
			if (!IsAvailable)
				return;

			AddCallback("onGameLoaded", callback);
			CallFromIframe("onGameLoaded");
		}
	}
}