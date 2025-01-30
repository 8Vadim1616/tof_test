using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Tower

{
    public class UserTower
    {
        public List<UserFloor> Floors { get; private set; } = new List<UserFloor>();
        public List<FloorItem> Items { get; private set; } = new List<FloorItem>();
        
        public List<TowerLevel> Levels { get; private set; } = new List<TowerLevel>();
        
        /**
        [JsonProperty("numberOfFloors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public int NumberOfFloors { get; private set; }
        */
      

        public UserTower()
        {
            
        }
        
        public TowerLevel GetLevel(int level)
        {
            return Levels.Find(towerLevel => towerLevel.Level == level);
        }        
    }
}