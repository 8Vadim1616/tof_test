namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class TestAddItemOperation : BaseApiOperation<TestAddItemOperation.Request, BaseApiResponse>
	{
		public TestAddItemOperation(int itemId, int cnt)
		{
			SetRequestObject(new Request()
			{
				id = itemId,
				cnt = cnt
			});
		}

		public class Request : BaseApiRequest
		{
			public int id;
			public int cnt;
			
			public Request() : base("test_get")
			{
			}
		}
	}
}