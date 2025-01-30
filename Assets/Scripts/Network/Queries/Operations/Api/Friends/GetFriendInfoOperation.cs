namespace Assets.Scripts.Network.Queries.Operations.Api.Friends
{
	public class GetFriendInfoOperation : BaseApiOperation<GetFriendInfoOperation.Request, BaseApiResponse>
	{
		public override bool NeedCheckInternet => true;
		public override bool NeedShowWindowError => true;

		public GetFriendInfoOperation(string fid)
		{
			SetRequestObject(new Request { fid = fid });
		}

		public class Request : BaseApiRequest
		{
			public string fid;

			public Request() : base("frndinfo")
			{

			}
		}
	}
}