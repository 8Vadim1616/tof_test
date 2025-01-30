using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.Items;

namespace Assets.Scripts.Static.Shop
{
    public class ShopItems : StaticCollectionCode<ShopItem>
    {
        public ShopItems(Dictionary<int, ShopItem> data) : base(data)
        {
        }
		
		public ShopItem GetRewardOffer(Item needItem, bool IsAd)
		{
			return All.Values.FirstOrDefault(s => s.Item == needItem && s.IsAd == IsAd && s.Type == ShopType.AD);
		}

		public List<ShopItem> GetAllByItem(Item item)
		{
			return All.Where(s => s.Value.Item == item)
					  .Select(shopItem => shopItem.Value).ToList();
		}

        public ShopItem GetByItem(Item item)
		{
			return All.Where(s => s.Value.Item == item)
					  .Select(shopItem => shopItem.Value).FirstOrDefault();
		}
		
		public ShopItem GetByItemWithAd(Item item, bool isAd)
		{
			return All.Where(s => s.Value.Item == item && s.Value.IsAd == isAd)
					  .Select(shopItem => shopItem.Value).FirstOrDefault();
		}
    }
}