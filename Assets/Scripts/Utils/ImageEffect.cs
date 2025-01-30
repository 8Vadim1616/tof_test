using UnityEngine;

namespace Assets.Scripts.Utils
{
	[RequireComponent(typeof(Camera))]
	public abstract class ImageEffect : MonoBehaviour
	{
		[SerializeField] protected Shader Shader;
		[SerializeField] private Material material;

		[HideInInspector] protected Camera Camera;

		protected virtual void Start()
		{
			//Camera cache
			Camera = GetComponent<Camera>();
		}

		protected Material Material
		{
			get
			{
				if (material == null)
				{
					material = new Material(Shader);
					material.hideFlags = HideFlags.HideAndDontSave;
				}
				return material;
			}
		}

		protected virtual void OnDisable()
		{
			if (material)
			{
				DestroyImmediate(material);
			}
		}
	}
}