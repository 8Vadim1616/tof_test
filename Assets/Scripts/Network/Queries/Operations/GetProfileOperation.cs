using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class GetProfileOperation : BaseApiOperation<GetProfileOperation.GetProfileRequest, GetProfileOperation.GetProfileResponse>
    {
        public class GetProfileRequest : BaseApiRequest
        {
            /* 
             *  * Input: sn - socnet str
                * Input: muid - muid
                * Input: mauth_key - auth key
                * Input: [hw] - client hardware string
            * */

            [JsonProperty("sn")]
            public string SocialNetwork { get; set; }

            [JsonProperty("muid")]
            public string MobileUid { get; set; }

            [JsonProperty("mauth_key")]
            public string MobileAuthKey { get; set; }

            [JsonProperty("hw", NullValueHandling = NullValueHandling.Ignore)]
            public string Hardware { get; set; }

            [JsonProperty("ver")]
            public string Version { get; set; }

            public virtual void PrepareToSend()
            {
                Version = Application.version;
            }

            public GetProfileRequest() : base("player.profile")
            {
            }
        }

        public class GetProfileResponse : BaseApiResponse
        {

            [JsonProperty("uid")]
            public string Uid { get; set; }

            [JsonProperty("auth_key")]
            public string AuthKey { get; set; }
            /*
             *  * Output: uid - local user id
                * Output: auth_key - local auth key
             * */
        }

        public GetProfileOperation(string pSocialNetwork, string pMobileUid, string pMobileAuthKey, string pHardware = null) : base()
        {
            SetRequestObject(new GetProfileRequest
            {
                SocialNetwork = pSocialNetwork,
                MobileUid = pMobileUid,
                MobileAuthKey = pMobileAuthKey,
                Hardware = pHardware
            });
        }

        public override string GetRequestFile()
        {
            return "api";
        }
    }
}
