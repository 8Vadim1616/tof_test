using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	public class GameObjectRotateInstantiator : MonoBehaviour
	{
		[Range(1, 100)]
		[SerializeField] private int totalObjects;

		public void Clone()
		{
			CreateClones();
		}

		public GameObjectRotateInstantiator[] CloneEditor()
		{
			var extra = CreateClones();
			return extra.Select(x => x.GetComponent<GameObjectRotateInstantiator>()).ToArray();
		}

		private List<GameObject> CreateClones()
		{
			var angleStep = totalObjects > 1 ? 360f / totalObjects : 0f;
			var list = new List<GameObject>(totalObjects - 1);
			for (int i = 1; i < totalObjects; i++)
			{
				var inst = Instantiate(gameObject, transform.parent);
				//Destroy(inst.GetComponent<GameObjectRotateInstantiator>());
				inst.transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.Add(z: i * angleStep));
				inst.transform.name = transform.name + i;
				list.Add(inst);
			}

			return list;
		}
	}
}