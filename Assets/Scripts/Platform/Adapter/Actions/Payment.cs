using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Adapter.Actions
{
	public class Payment : ISocialAction
	{
		public string Pname { get; private set; }
		public string Pdesc { get; private set; }
		public string Pid { get; private set; }
		public float Amt { get; private set; }
		public string Xbank { get; private set; }
		public JObject OtherParams { get; private set; }

		public Payment(string pname,
		               string pdesc,
		               string pid,
					   float amt = 0,
					   JObject otherParams = null)
		{
			Pname = pname;
			Pdesc = pdesc;
			Amt = amt;
			Pid = pid;
			OtherParams = otherParams;
		}

		public override string ToString()
		{
			return "";
		}
	}
}