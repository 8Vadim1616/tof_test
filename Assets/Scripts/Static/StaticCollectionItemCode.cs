using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static
{
    public class StaticCollectionItemCode : StaticCollectionItem
    {
        [JsonProperty("#id")]
        public string ModelId { get; set; }
    }
}