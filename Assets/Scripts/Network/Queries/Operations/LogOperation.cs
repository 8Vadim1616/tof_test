using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class LogOperation : BaseApiOperation<LogOperation.Request, LogOperation.Response>
    {
		public override bool NeedCheckInternet => false;
		public override bool NeedShowWindowError => false;

		public LogOperation(string action, Dictionary<string, object> data)
        {
            SetRequestObject(new Request() {action = action, data = data});
        }

		public override string ToString()
		{
			return $"{base.ToString()} [{RequestObject.action}]";
		}

        public class Request : BaseApiRequest
        {
            [JsonProperty("act")]
            public string action;

            [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<string, object> data { get; set; }

            public Request() : base("fllogs")
            {
            }
        }

        public class Response : BaseApiResponse
        {
        }
    }
}