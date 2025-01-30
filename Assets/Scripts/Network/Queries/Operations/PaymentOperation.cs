using Assets.Scripts.Network.Queries.Operations.Api;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class PaymentOperation : BaseApiOperation<PaymentOperation.Request, BaseApiResponse>
    {
		public PaymentOperation(int pos, string sign, string data, string productInfo, string transactionId)
		{
			SetRequestObject(new Request(pos, sign, data, productInfo, transactionId));
		}

        public class Request : BaseApiRequest
        {
            public int pos;
            public string sign;
            public string data;
			public string transaction_id;

			[JsonProperty("product_info")]
			public string ProductInfo { get; set; }

			public Request(int pos, string sign, string data, string productInfo, string transactionId) : base("payment")
			{
				this.pos = pos;
				this.sign = sign;
				this.data = data;

				ProductInfo = productInfo;
				transaction_id = transactionId;
			}
        }
    }
}