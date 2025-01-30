namespace Assets.Scripts.Network.Queries.Operations.Api.GamePlay
{
	public class ArtifactUpgradeOperation : BaseApiOperation<ArtifactUpgradeOperation.Request, BaseApiResponse>
	{
		public ArtifactUpgradeOperation(int id)
		{
			SetRequestObject(new Request()
			{
				id = id
			});
		}

		public class Request : BaseApiRequest
		{
			public int id;
			
			public Request() : base("artifact_upgrade")
			{
			}
		}
	}
}