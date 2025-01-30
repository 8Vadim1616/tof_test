using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Ability

{
    public class UserAbilityItem
    {
        [JsonProperty("id")]
        public string Id { get; private set; }
        
        
        [JsonProperty("name")]
        public string Name { get; private set; }

        /**
        [JsonProperty("mode")]
        public string Mode { get; private set; }
        
        [JsonProperty("#id")]
        public string SharpId { get; private set; }
        */
        
        public UserAbilityItem()
        {
            
        }
        

        
    }
}