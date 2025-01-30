using Assets.Scripts.User.MetaPayments;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.MetaPayment
{
	public class AddMetaPaymentOperation : BaseApiOperation<AddMetaPaymentOperation.Request, BaseApiResponse>
	{
		public AddMetaPaymentOperation(User.MetaPayments.MetaPayment metaPayment) =>
			SetRequestObject(new Request
			{
				MetaPayment = metaPayment,
			});

		public class Request : BaseApiRequest
		{
			[JsonProperty("data")]
			public User.MetaPayments.MetaPayment MetaPayment;

			public Request() : base("add_pp") { }
		}
	}
}