using System.Collections.Generic;

namespace Assets.Scripts.Static.Bank
{
	public class StaticBankItems : StaticCollection<StaticBankItem>
	{
		public StaticBankItems(Dictionary<int, StaticBankItem> data) : base(data)
		{
			foreach (var kv in All)
			{
				kv.Value.Id = kv.Key;
			}
		}
	}
}