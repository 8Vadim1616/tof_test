using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class CheckPaymentHuaweiOperation : BaseApiOperation<CheckPaymentHuaweiOperation.Request, CheckPaymentHuaweiOperation.Response>
	{
		public override bool NeedCheckInternet => true;
		public override bool NeedShowWindowError => true;

		public CheckPaymentHuaweiOperation(int pos, string orderId, string purchaseToken, string productId)
        {
            SetRequestObject(new Request(pos, orderId, purchaseToken, productId));
        }

        public class Request : BaseApiRequest
        {
			public int pos;
			public string orderId;
			public string purchase_token;
			public string productId;

            public Request(int pos, string orderId, string purchaseToken, string productId) : base("check_payment")
            {
				this.pos = pos;
				this.orderId = orderId;
				this.purchase_token = purchaseToken;
				this.productId = productId;
			}
		}

		public class Response : BaseApiResponse
		{
			[JsonProperty("was_pay", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public bool WasAlreadyProvided;
		}
	}
}