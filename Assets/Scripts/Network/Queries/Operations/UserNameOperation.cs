using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
	public class UserNameOperation : BaseApiOperation<UserNameOperation.Request, BaseApiResponse>
    {
        public UserNameOperation(UserNameOperation.Request request)
        {
            SetRequestObject(request);
        }

        public class Request : BaseApiRequest
        {
            [JsonProperty("fn", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string FirstName;
            
            [JsonProperty("ln", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string LastName;

            [JsonProperty("advert_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string AdvertId;
            
            [JsonProperty("fcm_token", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string FirebaseToken;
            
            [JsonProperty("af_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string AppsFlyerId;

			[JsonProperty("avatar_id")]
			public int? AvatarId;

			[JsonProperty("frame_id")]
			public int? AvatarFrameId;

			[JsonProperty("lang", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string Lang;

            [JsonProperty("tz", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public int TimeZoneOffset;

            public Request() : base("uname")
            {

            }
        }
    }
}