namespace Assets.Scripts.Platform.Adapter.Data
{
	public class TransactionQueueData
	{
		public int PositionId { get; private set; }
		public string Signature { get; private set; }

		public TransactionQueueData(int positionId, string sig)
		{
			PositionId = positionId;
			Signature = sig;
		}
	}
}