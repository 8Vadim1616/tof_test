using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Assets.Scripts.Libraries.RSG;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Network
{
	public class AbstractAdapterNetworkManager
	{
		private string _server = "";
		private string _entryPoint = "";
		private string _secret = "";

		public AbstractAdapterNetworkManager(string server, string entryPoint, string secret = null)
		{
			_server = server;
			_entryPoint = entryPoint;
			_secret = secret;
		}

		public void SetServer(string server, string entryPoint)
		{
			_server = server;
			_entryPoint = entryPoint;
		}

		protected virtual Promise<string> POSTGET(AdapterNetworkQuery query)
		{
			var promise = new Promise<string>();
			WWWForm data = null;

			var args = query.GetParams != null ? query.GetParams : new Dictionary<string, string>();

			AddBaseParams(args);

			string gets = args.Select(x => x.Key + "=" + x.Value).Aggregate((s1, s2) => s1 + "&" + s2);

			if (query.PostParams != null)
			{
				data = new WWWForm();
				data.AddField("post", 1);

				if (query.PostParams != null)
				{
					foreach (var val in query.PostParams)
					{
						data.AddField(val.Key, val.Value);
					}
				}

				GetBasePost(data);
			}

			var toUrl = _server;
			toUrl += query.EntryPoint != null ? query.EntryPoint : _entryPoint;
			toUrl += "?" + gets;

			send();

			return promise;

			void after(WWW www)
			{
				string errorString = null;

				if (www.error != null)
				{
					if (query.Iteration < 3)
					{
						GameLogger.warning("Server does not respond : iteration=" + query.Iteration + " error=" + www.error);

						query.OnErrorIteration();
						send();
					}
					else
					{
						errorString = www.error;

						GameLogger.info("Server request: " + toUrl);
						GameLogger.warning("Server does not respond : "   + www.error);
					}
				}
				else
				{
					if (www.text == null || www.text.Length == 0)
					{
						GameLogger.info("Server request: " + toUrl);
						GameLogger.warning("Server error : "              + www.text);

						errorString = www.error;
					}
					else
					{
						promise.Resolve(www.text);
					}
				}

				www.Dispose();

				if (errorString != null)
					promise.Reject(new Exception(errorString));
			}

			void send()
			{
				GameLogger.info("Server request: " + toUrl);

				var sendPromise = new Promise<WWW>();

				sendPromise
							   .Then(response =>
								{
									WWW www = (WWW) response;
									after(www);
								});

				Utils.Utils.StartCoroutine(_send());

				IEnumerator _send()
				{
					WWW www = null;

					if (data != null)
						www = new WWW(toUrl, data.data); // POST
					else
						www = new WWW(toUrl); // GET

					yield return www;
					sendPromise.Resolve(www);
				}
			}
		}

		public virtual Promise<JToken> SendToSN(string method, JObject gets = null)
		{
			Promise<JToken> promise = new Promise<JToken>();

			if (gets == null)
				gets = new JObject();

			AddSNArguments(gets);

			var dict = gets.ToObject<Dictionary<string, string>>();
			var query = new AdapterNetworkQuery(method, dict);
			query.SetToSN(true);

			POSTGET(query)
						   .Then(
								 data =>
								 {
									 try
									 {
										 var obj = JToken.Parse(data);
										 promise.Resolve(obj);
									 }
									 catch (Exception e)
									 {
										 promise.Reject(new Exception("SN error", e));
									 }
								 });

			return promise;
		}

		protected virtual void AddBaseParams(Dictionary<string, string> gets)
		{

		}

		protected virtual void GetBasePost(WWWForm Data)
		{

		}

		protected virtual void AddSNArguments(JObject getArgs)
		{

		}

		protected virtual string SignQuery(string par)
		{
			var data = par.Split('&');
			Dictionary<string, string> query = new Dictionary<string, string>();
			foreach (string val in data)
			{
				string[] p = val.Split('=');
				query[p[0]] = p[1];
			}

			var sortedDict = new SortedDictionary<string, string>(query);
			var outstring = "";
			foreach (var kvp in sortedDict)
				outstring += kvp.Key + "=" + kvp.Value;

			outstring += _secret;

			MD5 md5Hasher = MD5.Create();

			byte[] md5data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(outstring));
			StringBuilder sBuilder = new StringBuilder();
			for (int i = 0; i < md5data.Length; i++)
			{
				sBuilder.Append(md5data[i].ToString("x2"));
			}

			return par + "&sig=" + sBuilder.ToString();
		}

		protected virtual string Signature(Dictionary<string, string> dict)
		{
			var sortedDict = new SortedDictionary<string, string>(dict);
			var outstring = "";
			foreach (var kvp in sortedDict)
				outstring += kvp.Key + "=" + kvp.Value;

			outstring += _secret;

			MD5 md5Hasher = MD5.Create();

			byte[] md5data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(outstring));
			StringBuilder sBuilder = new StringBuilder();
			for (int i = 0; i < md5data.Length; i++)
			{
				sBuilder.Append(md5data[i].ToString("x2"));
			}

			return sBuilder.ToString();
		}
	}
}