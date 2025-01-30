using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Tower

{
    public class UserFloor
    {

        [JsonProperty("level")]
        public int Level { get; private set; }

        [JsonProperty("count", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int Count { get; private set; }
        
        [JsonProperty("exit")]
        public int Exit { get; private set; }
        
        [JsonProperty("type")]
        public UserFloorType Type { get; private set; }

        [JsonProperty("actions")]
        public List<UserFloorAction> Actions { get; private set; } = new List<UserFloorAction>();        
        
        public UserFloor()
        {
            
        }
    }
}