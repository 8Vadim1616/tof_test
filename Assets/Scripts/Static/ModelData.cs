using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static
{
    public class ModelData
    {
        [JsonProperty("Levels", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public JToken Levels { get; set; }
    }
}