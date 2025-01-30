
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Actions
{
	public class FBViralGraph : ISocialAction
	{
		public string Action { get; private set; }
		public JObject ExtraParams { get; private set; }
		public int Id { get; private set; }
		public string Mess { get; private set; }
		public string Obj { get; private set; }

		public FBViralGraph(string action, string obj, int id, JObject extraParams = null, string message = "")
		{
			Id = id;
			Action = action;
			Obj = obj;
			ExtraParams = extraParams;
			Mess = message;
		}

		public string ToString()
		{
			return "id="            + Id     +
			       "; action="      + Action +
			       "; obj="         + Obj    +
			       "; userMessage=" + Mess;
		}
	}
}