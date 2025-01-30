using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.Friends
{
	/// <summary>Получаем информацию по игрокам по локальным уидам</summary>
	public class GetUidsOperation : BaseApiOperation<GetUidsOperation.Request, BaseApiResponse>
	{
		public override bool NeedCheckInternet => true;
		public override bool NeedShowWindowError => true;

		public GetUidsOperation(List<string> uids)
		{
			SetRequestObject(new Request { FriendUids = uids });
		}

		public class Request : BaseApiRequest
		{
			[JsonProperty("fuids")]
			public List<string> FriendUids;

			public Request() : base("getuids")
			{

			}
		}
	}
}