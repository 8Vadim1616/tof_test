using Assets.Scripts.Static.Bank;

namespace Assets.Scripts.Network.Queries.Operations.Api
{
	public class BankItemBuyOperation : BaseApiOperation<BankItemBuyOperation.Request, BaseApiResponse>
	{
		public BankItemBuyOperation(UserBankItem bankItem)
		{
			SetRequestObject(new Request()
			{
				BankItem = bankItem
			});
		}

		public class Request : BaseApiRequest
		{
			public UserBankItem BankItem;
			public Request() : base("buy_bank_save")
			{
			}
		}
	}
}