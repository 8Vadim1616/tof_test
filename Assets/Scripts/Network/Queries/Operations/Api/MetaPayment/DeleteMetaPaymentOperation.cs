using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.MetaPayment
{
	public class DeleteMetaPaymentOperation : BaseApiOperation<DeleteMetaPaymentOperation.Request, BaseApiResponse>
	{
		public DeleteMetaPaymentOperation(string productId) =>
			SetRequestObject(new Request { ProductId = productId });

		public class Request : BaseApiRequest
		{
			[JsonProperty("product_id")]
			public string ProductId;

			public Request() : base("del_pp") { }
		}
	}
}