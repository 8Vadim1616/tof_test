using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class ShowBankPosOperation : BaseApiOperation<ShowBankPosOperation.Request, BaseApiResponse>
    {
        public ShowBankPosOperation(Dictionary<string, object> data)
        {
            SetRequestObject(new Request() {data = data});
        }

        public class Request : BaseApiRequest
        {
            [JsonProperty("data")]
            public Dictionary<string, object> data;

            public Request() : base("showbankpos")
            {
            }
        }
    }
}