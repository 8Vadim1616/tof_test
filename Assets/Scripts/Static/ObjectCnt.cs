using Newtonsoft.Json;

namespace Assets.Scripts.Static
{
	public class ObjectCnt<T>
	{
		[JsonProperty("cnt", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public T Count { get; set; }
	}
	
	public class ObjectId<T>
	{
		[JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public T Id { get; set; }
	}
}