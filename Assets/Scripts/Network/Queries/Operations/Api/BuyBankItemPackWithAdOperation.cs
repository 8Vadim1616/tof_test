namespace Assets.Scripts.Network.Queries.Operations.Api
{
	public class BuyBankItemPackWithAdOperation : BaseApiOperation<BuyBankItemPackWithAdOperation.Request, BaseApiResponse>
	{
		public BuyBankItemPackWithAdOperation(int packId)
		{
			SetRequestObject(new Request() { packId = packId });
		}

		public class Request : BaseApiRequest
		{
			public int packId;
			public Request() : base("buy_bank_pack_advert")
			{
			}
		}
	}
}