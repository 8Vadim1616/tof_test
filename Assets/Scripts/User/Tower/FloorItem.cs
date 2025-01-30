using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Tower

{
    public class FloorItem
    {
        [JsonProperty("item")]
        public int Id { get; private set; }

        [JsonProperty("value")]
        public int Value { get; private set; }
        
        public FloorItem()
        {
            
        }
        

        
    }
}