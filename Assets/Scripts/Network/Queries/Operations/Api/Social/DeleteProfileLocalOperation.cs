namespace Assets.Scripts.Network.Queries.Operations.Api.Social
{
	public class DeleteProfileLocalOperation : BaseApiOperation<DeleteProfileLocalOperation.Request, BaseApiResponse>
	{
		public override bool NeedLog => false;

		public DeleteProfileLocalOperation()
		{
			SetRequestObject(new Request());
		}

		public class Request : BaseApiRequest
		{
			public Request() : base("remove_local")
			{

			}
		}
	}
}