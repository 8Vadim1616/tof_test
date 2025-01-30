namespace Assets.Scripts.Network.Queries.Operations.Api
{
	public class LoadUserOperation : BaseApiOperation<LoadUserOperation.Request, BaseApiResponse>
	{
		public LoadUserOperation()
		{
			SetRequestObject(new Request());
		}
		public class Request : BaseApiRequest
		{
			public Request() : base("load_profile") { }
		}
	}
}
