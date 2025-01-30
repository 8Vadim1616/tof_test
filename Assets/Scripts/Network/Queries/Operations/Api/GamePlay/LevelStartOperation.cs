using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.Static.Tower;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class LevelStartOperation : BaseApiOperation<LevelStartOperation.Request, LevelStartOperation.Response>
	{
		public LevelStartOperation()
		{
			SetRequestObject(new Request());
		}

		public class Request : BaseApiRequest
		{
			public Request() : base("tower.start")
			{
			}
		}
		
		public class Response : BaseApiResponse
		{
			public UserAbility Ability;
			public UserTower Tower;
		}
		
	}
}