using Assets.Scripts.Network.Queries.Operations;

public class FBDeleteConfirmOperation : BaseApiOperation<FBDeleteConfirmOperation.Request, BaseApiResponse>
{
	public override bool NeedCheckInternet => true;
	public override bool NeedShowWindowError => true;

	public FBDeleteConfirmOperation()
	{
		SetRequestObject(new Request());
	}

	public class Request : BaseApiRequest
	{
		public Request() : base("fb_delete")
		{

		}
	}
}