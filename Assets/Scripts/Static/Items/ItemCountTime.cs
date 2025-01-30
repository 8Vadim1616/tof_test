using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Assets.Scripts.Static.Items
{
    public class ItemCountTime : ItemCount
    {
        [JsonProperty("tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public long Time { get; private set; }
    }
}
