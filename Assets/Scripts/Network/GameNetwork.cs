using System;
using Assets.Scripts.BuildSettings;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.Network
{
	public class GameNetwork : IDisposable
	{
		public const string TAG = "[GameNetwork] ";

		public QueryManager QueryManager { get; private set; }


		public GameNetwork()
		{
		}

		public void Init()
		{
			if(QueryManager == null)
				QueryManager = new QueryManager();
		}

		public bool NetworkReachable => Application.internetReachability != NetworkReachability.NotReachable;

		public void Dispose()
		{
		}
	}
}