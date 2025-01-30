using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.MetaPayment
{
	public class GetClosingOfferMetaPaymentOperation : BaseApiOperation<GetClosingOfferMetaPaymentOperation.Request, GetClosingOfferMetaPaymentOperation.Response>
	{
		public override bool NeedLog => false;

		public GetClosingOfferMetaPaymentOperation(int offerId) =>
			SetRequestObject(new Request
			{
				OfferId = offerId,
			});

		public class Request : BaseApiRequest
		{
			[JsonProperty("offer_id")]
			public int OfferId;

			public Request() : base("get_pp_close") { }
		}

		public class Response : BaseApiResponse
		{
			[JsonProperty("products")]
			public string[] Products;
		}
	}
}