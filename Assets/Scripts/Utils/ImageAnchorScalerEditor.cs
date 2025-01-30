#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    [CustomEditor(typeof(ImageAnchorsScaler))]
    public class ImageAnchorScalerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var t = (ImageAnchorsScaler) target;

            var buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = new Color(0, .3f, 0); 
            buttonStyle.fontStyle = FontStyle.Bold;
            buttonStyle.fontSize = 26;

            buttonStyle.hover.textColor = new Color(.05f, .45f, .05f);
            buttonStyle.active.textColor = new Color(.1f, .55f, .1f);

            if (GUILayout.Button("Обновить якоря", buttonStyle, GUILayout.MinHeight(60)))
            {
                t.SetupAnchors();
            }

            t.includeSelf = EditorGUILayout.Toggle("Include self", t.includeSelf);
        }

    }
}
#endif