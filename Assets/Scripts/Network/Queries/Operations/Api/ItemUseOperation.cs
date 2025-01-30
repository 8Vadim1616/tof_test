using Assets.Scripts.Static.Items;

namespace Assets.Scripts.Network.Queries.Operations.Api
{
	public class ItemUseOperation : BaseApiOperation<ItemUseOperation.Request, BaseApiResponse>
	{
		public ItemUseOperation(Item item, float count)
		{
			SetRequestObject(new Request { id = item.Id, cnt = count });
		}

		public class Request : BaseApiRequest
		{
			public int id;
			public float cnt;

			public Request() : base("itemuse")
			{
			}
		}
	}
}