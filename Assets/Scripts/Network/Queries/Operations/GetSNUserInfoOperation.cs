using Assets.Scripts.Platform.Adapter;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
	public class GetSNUserInfoOperation : Operation<GetSNUserInfoOperation.Request, GetSNUserInfoOperation.Response>
	{
		public class Request : BaseRequest
		{
			[JsonProperty("code", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string code { get; set; }

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

			/**
			 * Network User Id
			 */
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
			public int money;
		}

		public GetSNUserInfoOperation(string authToken, string authSn) : base()
		{
			var request = new Request { info = 1, sn = Game.Social.Network, AuthSn = authSn };
			request.Uid = Game.User.RegisterData.Uid;
			request.AuthKey = Game.User.RegisterData.AuthKey;
			request.MobileUid = Game.User.RegisterData.MobileUid;
			request.MobileAuthKey = Game.User.RegisterData.MobileAuthKey;

			request.AuthToken = authToken;

			SetRequestObject(request);
		}

		public override string GetRequestFile()
		{
			return "auth.php";
		}
	}
}