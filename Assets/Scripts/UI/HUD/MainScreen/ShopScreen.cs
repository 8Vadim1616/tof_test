using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class ShopScreen : MainScreenBase
	{
		[SerializeField] private ButtonTextIcon _itemPrefab;
		
		protected override void Init()
		{
			base.Init();

			_itemPrefab.SetActive(true);
			foreach (var item in Game.Static.Items.All.Values)
			{
				var itemView = Instantiate(_itemPrefab, _itemPrefab.transform.parent);
				itemView.Icon.LoadItemImage(item);
				var i = item;
				
				itemView.SetOnClick(() =>
				{
					var cnt = i.Type == ItemType.UNIT ? 1000 : 100000;
					Game.QueryManager.RequestPromise(new TestAddItemOperation(i.Id, cnt))
						.Then(r =>
						 {
							Game.ServerDataUpdater.Update(r);
						 });
				});
				
				item.UserReactive().Subscribe(_ =>
				{
					itemView.Text = $"{item.ModelId} ({item.UserAmount()})";
				}).AddTo(itemView.gameObject);
			}
			_itemPrefab.SetActive(false);
		}
	}
}