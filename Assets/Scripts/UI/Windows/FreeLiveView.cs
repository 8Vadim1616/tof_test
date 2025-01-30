using System;
using Assets.Scripts.UI.ControlElements;
using Assets.Scripts.Utils;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Windows
{
	public class FreeLiveView : MonoBehaviour
	{
		[SerializeField] TextMeshProUGUI _userName;
		[SerializeField] TextMeshProUGUI _desc;
		[SerializeField] ButtonText _btnTake;

		public void Init(string userName, Func<bool> onTake)
		{
			_userName.text = userName;
			_desc.text = "send_you_life".Localize();
			_btnTake.Text = "take".Localize();

			_btnTake.onClick.RemoveAllListeners();
			_btnTake.onClick.AddListener(() =>
			{
				if (onTake())
					_btnTake.onClick.RemoveAllListeners();
			});
		}
	}
}