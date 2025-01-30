namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class LevelUpOperation : BaseApiOperation<LevelUpOperation.Request, LevelUpOperation.Response>
	{
		public LevelUpOperation()
		{
			SetRequestObject(new Request());
		}

		public class Request : BaseApiRequest
		{
			public Request() : base("user_levelup")
			{
			}
		}
		
		public class Response : BaseApiResponse
		{
		}
	}
}