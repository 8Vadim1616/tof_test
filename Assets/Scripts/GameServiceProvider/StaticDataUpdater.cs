using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Queries.Operations.Api.StaticData;
using Assets.Scripts.Static;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.GameServiceProvider
{
	public class StaticDataUpdater
	{
		public static List<int> CheckedGroupsInCurrentSession = new List<int>();

		public IPromise Update()
		{
			var result = new Promise();
			Game.QueryManager.OnConnect.Then(() =>
			{
				Game.QueryManager.RequestPromise(new UpdateStaticDataOperation(), false)
					.Then(response =>
					 {
						 if (response.Files != null)
						 {
							 foreach (var kv in response.Files)
							 {
								 if (kv.Value.Data == null)
									 continue;

								 FileResourcesLoader.NoGroup().SaveFileWithVersion(kv.Key, kv.Value.Data, kv.Value.Ver.Value);
							 }
						 }

						 if (response.ModelData != null)
							 FileResourcesLoader.NoGroup().SaveFileWithVersion(FileResourcesLoader.MODEL_DATA_FILE, response.ModelData, response.ModelData.Version);

						 result.ResolveOnce();
						 Debug.Log("[StaticDataUpdater] All files updated");
					 });
			});

			return result;
		}
	}
}