using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.Friends
{
	/// <summary>Получаем информацию по игрокам по уидам из соцсети</summary>
	public class GetSnUidsOperation : BaseApiOperation<GetSnUidsOperation.Request, BaseApiResponse>
	{
		public override bool NeedCheckInternet => true;
		public override bool NeedShowWindowError => true;

		public GetSnUidsOperation(List<string> foreignUids, string socnet = null)
		{
			var needSocnet = socnet ?? Game.Social.Network;
			SetRequestObject(new Request { FriendUids = foreignUids, Socnet = needSocnet });
		}

		public class Request : BaseApiRequest
		{
			[JsonProperty("fuids")]
			public List<string> FriendUids;

			[JsonProperty("fsocnet")]
			public string Socnet;

			public Request() : base("getsnuids")
			{

			}
		}
	}
}