using Assets.Scripts.User.MetaPayments;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.MetaPayment
{
	public class GetMetaPaymentOperation : BaseApiOperation<GetMetaPaymentOperation.Request, GetMetaPaymentOperation.Response>
	{
		public GetMetaPaymentOperation(string productId) =>
			SetRequestObject(new Request
			{
				ProductId = productId,
			});

		public class Request : BaseApiRequest
		{
			[JsonProperty("product_id")]
			public string ProductId;

			public Request() : base("get_pp") { }
		}

		public class Response : BaseApiResponse
		{
			[JsonProperty("data")]
			public User.MetaPayments.MetaPayment MetaPayment;
		}
	}
}