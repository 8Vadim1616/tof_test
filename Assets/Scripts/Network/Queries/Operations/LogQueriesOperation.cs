using System.Collections.Generic;
using Assets.Scripts.Network.Queries.ServerObjects;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class LogQueriesOperation : BaseApiOperation<LogQueriesOperation.Request, BaseApiResponse>
    {
		public LogQueriesOperation(List<JObject> logs)
		{
			SetRequestObject(new Request(logs));
		}
        
		public class Request : BaseApiRequest
		{
			public List<JObject> logs;

			public Request(List<JObject> logs) : base("fllogs2")
			{
				this.logs = logs;
			}
		}
    }
}