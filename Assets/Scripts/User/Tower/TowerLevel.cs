using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Tower

{
    public class TowerLevel
    {
        [JsonProperty("level")]
        public int Level { get; private set; }

        [JsonProperty("exp")]
        public int Exp { get; private set; }
        
        public TowerLevel()
        {
            
        }
        

        
    }
}