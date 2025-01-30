using Newtonsoft.Json;

namespace Assets.Scripts.User.MetaPayments
{
	public class MetaPaymentWrapper
	{
		[JsonProperty("meta")]
		[JsonConverter(typeof(MetaPaymentConverter))]
		public MetaPayment MetaPayment;
	}
}