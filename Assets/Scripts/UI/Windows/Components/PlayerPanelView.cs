using Assets.Scripts.User;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI.Windows.Components
{
	public class PlayerPanelView : MonoBehaviour
	{
		[SerializeField] private Image _avatar;
		[SerializeField] private TMP_Text _name;
		[SerializeField] private TMP_Text _level;

		public void SetUser(UserData userData)
		{
			/** JAVA
			_name.text = userData.Nick.Value;
			_level.text = userData.Level.Value.Id.ToString();
			*/
		}
	}
}