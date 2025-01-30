using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Tower

{
    public class UserFloorType
    {
        [JsonProperty("id")]
        public int Id { get; private set; }

        [JsonProperty("#id")]
        public string SharpId { get; private set; }
        
        public UserFloorType()
        {
            
        }
        

        
    }
}