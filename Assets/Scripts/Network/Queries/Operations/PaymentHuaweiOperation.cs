namespace Assets.Scripts.Network.Queries.Operations
{
    public class PaymentHuaweiOperation : BaseApiOperation<PaymentHuaweiOperation.Request, BaseApiResponse>
    {
		public PaymentHuaweiOperation(int pos, string orderId, string purchaseToken, string productId)
		{
			SetRequestObject(new Request(pos, orderId, purchaseToken, productId));
		}

        public class Request : BaseApiRequest
        {
            public int pos;
			public string orderId;
            public string purchase_token;
            public string productId;

            public Request(int pos, string orderId, string purchaseToken, string productId) : base("payment")
            {
                this.pos = pos;
                this.orderId = orderId;
                this.purchase_token = purchaseToken;
                this.productId = productId;
			}
        }
    }
}