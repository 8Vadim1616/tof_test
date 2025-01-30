using Assets.Scripts.Utils;
using Newtonsoft.Json;

namespace Assets.Scripts.Static.Bank
{
    public class BankItem
    {
		[JsonProperty("tier")]
		public int Tier { get; private set; }

		[JsonProperty("code")]
		public string WstoreCode { get; private set; }

		[JsonProperty("s")]
		public bool IsSubscription { get; private set; }

		public string Id => (_isReal ? "" : Game.Settings.IAPPrefix) + _id;

        private string _id;
		private bool _isReal;

		public BankItem()
		{

		}

		public BankItem(string id, bool isReal)
		{
			SetId(id, isReal);
		}

		public static BankItem Of(UserBankItem userBankItem)
		{
			var result = new BankItem(userBankItem.ProductId, true);
			result.Tier = userBankItem.Tier;
			result.IsSubscription = userBankItem.IsSubscription;
			return result;
		}
		
		public static BankItem OfOldPos(UserBankItem userBankItem)
		{
			if (userBankItem.OldProductId.IsNullOrEmpty())
				return null;
			
			var result = new BankItem(userBankItem.OldProductId, true);
			result.Tier = userBankItem.Tier;
			result.IsSubscription = userBankItem.IsSubscription;
			return result;
		}

        public void SetId(string id, bool isReal)
        {
			_id = id;
            _isReal = isReal;
		}
	}
}