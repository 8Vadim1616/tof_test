using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.General;

namespace Assets.Scripts.UI.HUD.Widgets
{
	public class ItemCountList : AbstractItemListWidget<ItemCount, ItemCountView>
	{
		protected override void OnViewCreate(ItemCountView view, ItemCount itemCount)
		{
			view.SetItemCount(itemCount);
		}
	}
}