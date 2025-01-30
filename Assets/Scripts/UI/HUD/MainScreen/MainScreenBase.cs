using UnityEngine;

namespace Assets.Scripts.UI.HUD.MainScreen
{
	public class MainScreenBase : MonoBehaviour
	{
		public bool Inited { get; private set; }

		protected virtual void Init()
		{
			Inited = true;
		}
		
		public void Show()
		{
			gameObject.SetActive(true);
			
			if (!Inited)
				Init();
		}
		
		public void Hide()
		{
			gameObject.SetActive(false);	
		}
	}
}