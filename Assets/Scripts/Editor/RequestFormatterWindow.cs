using Assets.Scripts.Network.Queries;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Editor
{
	public class RequestFormatterWindow : EditorWindow
	{
		[MenuItem("Tools/Get Signature")]
		private static void ShowWindow() => GetWindow<RequestFormatterWindow>().Show();

		private string url = "";
		private string request = "";
		private string signature = "";

		private void OnGUI()
		{
			url = EditorGUILayout.TextField("URL: ", url);
			request = EditorGUILayout.TextField("POST JSON: ", request);
			signature = EditorGUILayout.TextField("Signature: ", signature);

			if (GUILayout.Button("Generate"))
				signature = Assets.Scripts.Utils.Utils.Hash(request + QueryManager.SALT);
		}
	}
}
