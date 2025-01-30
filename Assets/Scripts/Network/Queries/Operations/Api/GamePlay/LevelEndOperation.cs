using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Items;
using Gameplay.Components;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class LevelEndOperation : BaseApiOperation<LevelEndOperation.Request, LevelEndOperation.Response>
	{
		public LevelEndOperation(GameStats stats)
		{
			SetRequestObject(new Request{Stats = stats});
		}

		public class Request : BaseApiRequest
		{
			[JsonProperty("stats")]
			public GameStats Stats;
			
			public Request() : base("tower.end")
			{
			}
		}
		
		public class Response : BaseApiResponse
		{
		}
	}
}