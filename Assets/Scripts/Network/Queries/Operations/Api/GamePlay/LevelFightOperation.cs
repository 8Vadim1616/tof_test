using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.Static.Items;
using Gameplay.Components;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class LevelFightOperation : BaseApiOperation<LevelFightOperation.Request, LevelFightOperation.Response>
	{
		public LevelFightOperation(string id)
		{
			SetRequestObject(new Request{Id = id});
		}

		public class Request : BaseApiRequest
		{
			[JsonProperty("id")]
			public string Id;
			
			public Request() : base("tower.fight")
			{
			}
		}
		
		public class Response : BaseApiResponse
		{
			//public UserAbility Ability;
		}
	}
}