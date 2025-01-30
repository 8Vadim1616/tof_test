using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.ServerObjects
{
    public class ServerAdmin
    {
        [JsonProperty("level_result", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? LevelResult;
    }
}