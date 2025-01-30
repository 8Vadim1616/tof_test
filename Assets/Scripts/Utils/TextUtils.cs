using Assets.Scripts.Core.AssetsManager;
using Assets.Scripts.UI.Utils;
using Assets.Scripts.Libraries.RSG;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Utils
{
	public static class TextUtils
	{
		private static Gradient _goldenGradient;
		public static Gradient GoldenGradient
		{
			get
			{
				if (_goldenGradient == null)
				{
					_goldenGradient = new Gradient();
					_goldenGradient.colorKeys = new GradientColorKey[]
					{
					new GradientColorKey(new Color(.93f, 1f, 0f), .4f),
					new GradientColorKey(new Color(1f, 1f, 1f), .55f),
					new GradientColorKey(new Color(.93f, 1f, 0f), .7f),
					};
				}

				return _goldenGradient;
			}
		}

		public static IPromise MakeGoldenShineText(this TMP_Text target)
        {
			Promise loadPromise = new Promise();
			AssetsManager.Instance.Loader.LoadAndCache<Material>("Fonts/white_red")
				.Then(material =>
				{
					target.fontSharedMaterial = material;
					loadPromise.ResolveOnce();
				});

			TextColorEvalute evalute = target.gameObject.AddComponent<TextColorEvalute>();
			evalute.ColorGradient = GoldenGradient;

			return loadPromise;
		}

		public static void RemoveGoldenShineText(this TMP_Text target)
		{
			if (target.gameObject.TryGetComponent(out TextColorEvalute evalute))
				GameObject.Destroy(evalute);
		}
	}
}