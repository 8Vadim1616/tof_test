using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.Static.Items;
using Gameplay.Components;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class AbilityChooseOperation : BaseApiOperation<AbilityChooseOperation.Request, AbilityChooseOperation.Response>
	{
		public AbilityChooseOperation(string id)
		{
			SetRequestObject(new Request{Id = id});
		}

		public class Request : BaseApiRequest
		{
			[JsonProperty("id")]
			public string Id;
			
			public Request() : base("ability.choose")
			{
			}
		}
		
		public class Response : BaseApiResponse
		{
			public UserAbility Ability;
		}
	}
}