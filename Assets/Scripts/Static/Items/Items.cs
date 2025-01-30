using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Assets.Scripts.Utils;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Static.Items
{
    public class Items : StaticCollectionCode<Item>
    {
		public Items(JToken token) : base(token) { }

		public Items(Dictionary<int, Item> data) : base(data)
		{
			UnknownItem = new Item
			{
				Id = Money1.Id,
				ModelId = Money1.ModelId
			};
		}

        private Item money1;
        public Item Money1 => money1 ??= this[Item.MONEY1];
		
		private Item money2;
		public Item Money2 => money2 ??= this[Item.MONEY2];
		
		private Item mythic;
		public Item Mythic => mythic ??= this[Item.MYTHIC];
		
		private Item waveCoin;
		public Item WaveCoin => waveCoin ??= this[Item.WAVE_COIN];
		
		private Item waveSpecialCoin;
		public Item WaveSpecialCoin => waveSpecialCoin ??= this[Item.WAVE_SPECIAL_COIN];
		
		private Item exp;
		public Item Exp => exp ??= this[Item.EXP];

		public List<Item> GetAllByType(string type) => All.Values
            .Where(it => it.Type == type)
            .ToList();
	}
}