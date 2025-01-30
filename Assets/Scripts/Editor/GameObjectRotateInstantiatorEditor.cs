using Assets.Scripts.Utils;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
	[CustomEditor(typeof(GameObjectRotateInstantiator))]
	public class GameObjectRotateInstantiatorEditor : UnityEditor.Editor
	{
		private GameObjectRotateInstantiator Target { get; set; }
		public override void OnInspectorGUI()
		{
			Target = (GameObjectRotateInstantiator) target;

			DrawDefaultInspector();

			if (GUILayout.Button("Instantiate"))
			{
				var extras = Target.CloneEditor();
				foreach (var ex in extras)
				{
					DestroyImmediate(ex);
				}
			}
		}
	}
}