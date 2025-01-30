using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api.Social
{
	public class DeleteProfileOperation : BaseApiOperation<DeleteProfileOperation.Request, DeleteProfileOperation.Response>
	{
		public override bool NeedCheckInternet => true;
		public override bool NeedShowWindowError => true;

		public DeleteProfileOperation(string confirmText, string advertId = null)
		{
			SetRequestObject(new Request() { ConfirmText = confirmText, AdvertisingId = advertId });
		}

		public class Request : BaseApiRequest
		{
			[JsonProperty("confirm", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string ConfirmText { get; set; }

			[JsonProperty("advert_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string AdvertisingId { get; set; }

			public Request() : base("unlink")
			{

			}
		}

		public class Response : BaseApiResponse
		{
			[JsonProperty("success")]
			public bool Success { get; set; }
		}
	}
}