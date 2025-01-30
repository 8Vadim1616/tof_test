using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Static.Items;

namespace Assets.Scripts.Static.Shop
{
    public class ShopItemCount<T> where T : ShopItem
    {
        public T ShopItem { get; private set; }
        public int Count { get; private set; }

        public ShopItemCount(T shopItem, int count)
        {
            ShopItem = shopItem;
            Count = count;
        }
    }
}
