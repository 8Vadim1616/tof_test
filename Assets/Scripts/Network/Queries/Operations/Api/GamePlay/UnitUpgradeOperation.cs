namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class UnitUpgradeOperation : BaseApiOperation<UnitUpgradeOperation.Request, BaseApiResponse>
	{
		public UnitUpgradeOperation(int unitId)
		{
			SetRequestObject(new Request()
			{
				id = unitId
			});
		}

		public class Request : BaseApiRequest
		{
			public int id;
			
			public Request() : base("unit_upgrade")
			{
			}
		}
	}
}