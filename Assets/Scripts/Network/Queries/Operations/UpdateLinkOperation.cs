using Assets.Scripts.Network.Queries.ServerObjects;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations
{
	public class UpdateLinkOperation : Operation<UpdateLinkOperation.Request, UpdateLinkOperation.Response>
	{
		public class Request : BaseRequest
		{

			[JsonProperty("uid", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string Uid { get; set; }

			[JsonProperty("auth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AuthKey { get; set; }

			[JsonProperty("muid", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string MobileUid { get; set; }

			[JsonProperty("mauth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string MobileAuthKey { get; set; }

			[JsonProperty("need_profile", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public bool NeedProfile { get; set; }

			[JsonProperty("sn", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string sn { get; set; }
		}

		public class Response : BaseResponse
		{
			[JsonProperty("profile", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public ServerUser Profile { get; set; }
		}

		public UpdateLinkOperation(bool needProfile) : base()
		{
			var request = new Request { sn = Game.Social.Network };
			request.Uid = Game.User.RegisterData.Uid;
			request.AuthKey = Game.User.RegisterData.AuthKey;
			request.MobileUid = Game.User.RegisterData.MobileUid;
			request.MobileAuthKey = Game.User.RegisterData.MobileAuthKey;

			if (needProfile)
				request.NeedProfile = needProfile;

			SetRequestObject(request);
		}

		public override string GetRequestFile()
		{
			return "update_link.php";
		}
	}
}

