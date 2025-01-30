using UnityEngine;

namespace Assets.Scripts.Core.Sound
{
	public partial class SoundManager
	{
		#region GUI

		public void PlayBasicButtonClick() => PlaySound("bm_tap0");
		public void PlayPurchase() => PlaySound("mc_purchase");
		public void OnCollectInMain() => PlaySound("coc_unit_upgrade");
		public void WinOpen() => PlaySound("coc_window_open");
		public void WinClose() => PlaySound("coc_window_close");
		public void ConsumeUnit() => PlaySound("bm_object_drop");
		public void RepairStart() => PlaySound("construction_process_loop");
		public void RewardWindowOpened() => PlaySound("prilet_xp3");

		public void BossAttack() => PlaySound($"boss_{Random.Range(0, 4)}");
		public void BossDie() => PlaySound("mmooough");

		#endregion
	}
}