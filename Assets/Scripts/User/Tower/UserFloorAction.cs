using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Tower

{
    public class UserFloorAction
    {
        [JsonProperty("items")]
        public List<FloorItem> Items { get; private set; } = new List<FloorItem>();
        
        [JsonProperty("mode")]
        public string Mode { get; private set; }
        
        [JsonProperty("#id")]
        public string SharpId { get; private set; }
        
        public UserFloorAction()
        {
            
        }
        

        
    }
}