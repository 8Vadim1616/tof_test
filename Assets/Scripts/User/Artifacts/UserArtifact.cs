using System.Linq;
using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Static.Artifacts;
using Assets.Scripts.Static.Items;
using UniRx;

namespace Assets.Scripts.User.Artifacts
{
	public class UserArtifact
	{
		public Artifact Data { get; private set; }
		public IntReactiveProperty Level { get; } = new(1);
		public bool IsOwned { get; internal set; }
		public float Value { get; private set; }

		public UserArtifact(Artifact data)
		{
			Data = data;
		}
		
		public void Update(int level)
		{
			Level.Value = level;
			Value = Data.Bonus + Data.UpgradeBonus * (level - 1);
		}
		
		public void Upgrade()
		{
			Game.QueryManager.RequestPromise(new ArtifactUpgradeOperation(Data.Id))
				.Then(r =>
				 {
					 Game.ServerDataUpdater.Update(r);
				 });
		}
		
		public ItemCount[] GetUpgradeCost() => Data.GetLevelCost(Level.Value + 1);
		public ItemCount GetUpgradeMoney1() => Data.GetLevelCost(Level.Value + 1)?.FirstOrDefault(c => c.Item.IsMoney1);
		public ItemCount GetUpgradeCards() => Data.GetLevelCost(Level.Value + 1)?.FirstOrDefault(c => !c.Item.IsMoney1);
	}
}