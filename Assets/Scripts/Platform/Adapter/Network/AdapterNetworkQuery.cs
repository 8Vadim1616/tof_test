using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Network
{
	public class AdapterNetworkQuery
	{
		public string EntryPoint { get; private set; }
		public Dictionary<string, string> GetParams { get; private set; }
		public Dictionary<string, string> PostParams { get; private set; }
		public Action<JObject> OnComplete{ get; private set; }
		public Action<string> OnError { get; private set; }
		public int Iteration { get; private set; }
		public bool ToSN { get; private set; }

		public AdapterNetworkQuery(string entryPoint, Dictionary<string, string> getParams = null, Dictionary<string, string> postParams = null, Action<JObject> onComplete = null, Action<string> onError = null)
		{
			EntryPoint = entryPoint;
			GetParams = getParams;
			PostParams = postParams;
			OnComplete = onComplete;
			OnError = onError;
		}

		public void OnErrorIteration()
		{
			Iteration++;
		}

		public void SetIteration(int val)
		{
			Iteration = val;
		}
		public void SetToSN(bool val)
		{
			ToSN = val;
		}
	}
}