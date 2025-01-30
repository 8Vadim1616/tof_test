using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.Static.Items;
using Gameplay.Components;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class AbilitySelectOperation : BaseApiOperation<AbilitySelectOperation.Request, AbilitySelectOperation.Response>
	{
		public AbilitySelectOperation()
		{
			SetRequestObject(new Request());
		}

		public class Request : BaseApiRequest
		{
			public Request() : base("ability.select")
			{
			}
		}
		
		public class Response : BaseApiResponse
		{
		}
	}
}