using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.UI.Windows.Bank;
using Assets.Scripts.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.HUD.Panels
{
	public class SomeResourcePanel : MonoBehaviour
	{
		[SerializeField] protected TextMeshProUGUI amount;
		[SerializeField] protected BasicButton btnAdd;
		[SerializeField] protected Image icon;
		
		public void Init(Item item)
		{
			icon.LoadItemImage(item);
			Game.User.Items.GetReactiveProperty(item).Subscribe(_ =>
			{
				amount.text = item.UserAmount().ToKiloFormat();
			}).AddTo(this);
			
			btnAdd.SetOnClick(() => BankWindow.Of());
		}
	}
}