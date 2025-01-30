using System.Collections.Generic;

namespace Assets.Scripts.Static.Bank
{
	public class StaticBankPacks : StaticCollection<StaticBankPack>
	{
		public StaticBankPacks(Dictionary<int, StaticBankPack> data) : base(data)
		{
		}
	}
}