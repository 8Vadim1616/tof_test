using UnityEngine;

namespace Assets.Scripts.UI
{
	public interface IItemDropAnimated
	{
		void OnItemDropArrival();

		Vector3 GetPositionGlobal();
	}
}