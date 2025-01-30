using System.Collections.Generic;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Ability

{
    public class UserAbility
    {
        public List<UserAbilityItem> Select { get; private set; } = new List<UserAbilityItem>();
        public List<UserAbilityItem> List { get; private set; } = new List<UserAbilityItem>();
        
        public UserAbility()
        {
            
        }
    }
}