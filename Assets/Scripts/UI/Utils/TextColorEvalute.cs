using TMPro;
using UnityEngine;

namespace Assets.Scripts.UI.Utils
{
	[RequireComponent(typeof(TMP_Text))]
	public class TextColorEvalute : MonoBehaviour
	{
		[SerializeField] TMP_Text _target;

		public Gradient ColorGradient;
		public float TimeMultiplier = .85f;

		private readonly float _evalute = -0.001f;

		private void Start()
		{
			if (!_target)
				_target = GetComponent<TMP_Text>();
		}

		private void Update()
		{
			_target.ForceMeshUpdate();
			var mesh = _target.mesh;
			var vertices = mesh.vertices;

			Color[] colors = mesh.colors;

			for (int i = 0; i < _target.textInfo.characterCount; i++)
			{
				TMP_CharacterInfo c = _target.textInfo.characterInfo[i];

				int index = c.vertexIndex;

				float endTime = Time.time * TimeMultiplier;

				if (index + 3 < colors.Length && index + 3 < vertices.Length)
				{
					colors[index] = ColorGradient.Evaluate(Mathf.Repeat(endTime + vertices[index].x * _evalute, 1f));
					colors[index + 1] = ColorGradient.Evaluate(Mathf.Repeat(endTime + vertices[index + 1].x * _evalute + .15f, 1f));
					colors[index + 2] = ColorGradient.Evaluate(Mathf.Repeat(endTime + vertices[index + 2].x * _evalute + .1f, 1f));
					colors[index + 3] = ColorGradient.Evaluate(Mathf.Repeat(endTime + vertices[index + 3].x * _evalute, 1f));

					//colors[index] = Color.red;
					//colors[index + 1] = Color.green;
					//colors[index + 2] = Color.blue;
					//colors[index + 3] = Color.yellow;
				}
			}

			mesh.colors = colors;
			_target.canvasRenderer.SetMesh(mesh);
		}
	}
}