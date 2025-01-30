using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api
{
    public class SettingsSetOperation : BaseApiOperation<SettingsSetOperation.Request, BaseApiResponse>
    {
        public SettingsSetOperation(Dictionary<string, object> data)
        {
            SetRequestObject(new Request {data = data});
        }

        public class Request : BaseApiRequest
        {
            public Request() : base("sett")
            {

            }

            [JsonProperty("sett")]
            public Dictionary<string, object> data { get; set; }
        }
    }
}