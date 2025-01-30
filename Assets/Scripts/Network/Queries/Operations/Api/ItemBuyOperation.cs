using Assets.Scripts.Static.Shop;
using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations.Api
{
    public class ItemBuyOperation : BaseApiOperation<ItemBuyOperation.Request, BaseApiResponse>
    {
        public ItemBuyOperation(ShopItem shopItem, AdOptions options = null, int? buyCount = null, bool needDrop = false)
        {
            SetRequestObject(new Request(options)
			{
				id = shopItem.Id,
				count = buyCount,
				drop = needDrop
			});
        }

        public class Request : BaseApiRequest
        {
            public int id;

			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? count;

			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public bool drop;

			public Request(AdOptions options = null) : base("itembuy")
            {
				UpdateByAdOptions(options);
            }
        }

		public class Response : BaseApiResponse
		{
		}
	}
}
