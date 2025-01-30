using System.Collections.Generic;
using AppsFlyerSDK;
using Assets.Scripts.Platform.Mobile.Analytics.Partners;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class RegisterOperation : BaseApiOperation<RegisterOperation.RegisterRequest, RegisterOperation.RegisterResponse>
    {
        public class RegisterRequest : BaseApiRequest 
        {
            [JsonProperty("sn")]
            public string SocialNetwork { get; set; }
            
            [JsonProperty("advert_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string advertising_id { get; set; }
            
            [JsonProperty("model", DefaultValueHandling = DefaultValueHandling.Ignore)]
            public string model { get; set; }

            [JsonProperty("test", NullValueHandling = NullValueHandling.Ignore)]
            public string Test { get; set; }
			
			[JsonProperty("af", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public Dictionary<string, object> AppsFlayerData;
			
			[JsonProperty("af_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AppsFlyerId;

            [JsonProperty("ver")]
            public string Version { get; set; }

			public override void PrepareToSend()
            {
                base.PrepareToSend();
                Version = Application.version;
            }

			public RegisterRequest() : base("player.register")
			{
			}
			
        }

        public class RegisterResponse : BaseApiResponse
        {
            [JsonProperty("muid")]
            public string MobileUid { get; set; }

            [JsonProperty("mauth_key")]
            public string MobileAuthKey { get; set; }
			
			[JsonProperty("gdpr")]
			public bool NeedShowGDPR { get; set; }
		}

		public RegisterOperation(string pSocialNetwork,
								 string advertId,
								 string pTest = null) : base()
		{
			var request = new RegisterRequest
			{
				SocialNetwork = pSocialNetwork,
				advertising_id = advertId,
				Test = pTest,
				model = SystemInfo.deviceModel,
			};

			if (AppsFlyerPartner.ConversionData != null)
				request.AppsFlayerData = AppsFlyerPartner.ConversionData;

			request.AppsFlyerId = AppsFlyer.getAppsFlyerId();

			SetRequestObject(request);
		}

        public override string GetRequestFile()
        {
            return "api";
        }
    }
}
