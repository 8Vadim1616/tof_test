using System.Collections.Generic;
using Assets.Scripts.Network.Queries.Operations.Api.StaticData;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Platform.Adapter;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
	public class LinkToSNOperation : Operation<LinkToSNOperation.Request, LinkToSNOperation.Response>
	{
		public class Request : BaseRequest
		{
			[JsonProperty("auth_token", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AuthToken { get; set; }

			[JsonProperty("sn", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string sn { get; set; }

			[JsonProperty("auth_sn", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AuthSn { get; set; }

			[JsonProperty("info", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int info { get; set; }

			[JsonProperty("uid", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string Uid { get; set; }

			[JsonProperty("auth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AuthKey { get; set; }

			[JsonProperty("muid", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string MobileUid { get; set; }

			[JsonProperty("mauth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string MobileAuthKey { get; set; }
		}

		public class Response : BaseResponse
		{
			public string uid;
			public string auth_key;
			public int level;
			public string fn;
			public int rt;
			public int exp;
			public string avatar;

			[JsonProperty("files")] public Dictionary<string, FileWithVersionData> Files;

			[JsonProperty("profile", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public ServerUser Profile { get; set; }
		}

		public LinkToSNOperation(string curUid, string authToken, string authSn) : base()
		{
			var request = new Request { sn = Game.Social.Network, AuthSn = authSn };
			request.Uid = curUid;
			request.AuthKey = Game.User.RegisterData.AuthKey;
			request.MobileUid = Game.User.RegisterData.MobileUid;
			request.MobileAuthKey = Game.User.RegisterData.MobileAuthKey;

			//request.Version = Application.version;
			//request.Files = GetFilesToLoad(curGroup);

			request.AuthToken = authToken;

			SetRequestObject(request);
		}

		public override string GetRequestFile()
		{
			return "auth.php";
		}
	}
}