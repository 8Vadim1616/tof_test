using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif

namespace Assets.Scripts.UI.Utils
{
    [RequireComponent(typeof(RectTransform))]
    [System.Serializable]
    public class LayoutElementWithMaxValuesNew : LayoutElement
    {

        public float maxHeight;
        public float maxWidth;

        public bool useMaxWidth;
        public bool useMaxHeight;

        bool ignoreOnGettingPreferedSize;

        public override int layoutPriority
        {
            get => ignoreOnGettingPreferedSize ? -1 : base.layoutPriority;
            set => base.layoutPriority = value;
        }

        public override float preferredHeight
        {
            get
            {
                if (useMaxHeight)
                {
                    var baseValue = base.preferredHeight;

                    return (baseValue > maxHeight || baseValue < 0) ? maxHeight : baseValue;
                }
                else
                    return base.preferredHeight;
            }
            set => base.preferredHeight = value;
        }

        public override float preferredWidth
        {
            get
            {
                if (useMaxWidth)
                {
                    var baseValue = base.preferredWidth;

                    return baseValue > maxWidth ? maxWidth : baseValue;
                }
                else
                    return base.preferredWidth;
            }
            set => base.preferredWidth = value;
        }

    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LayoutElementWithMaxValuesNew), true)]
    [CanEditMultipleObjects]
    public class LayoutMaxSizeNewEditor : LayoutElementEditor
    {
		LayoutElementWithMaxValuesNew layoutMax;

        SerializedProperty maxHeightProperty;
        SerializedProperty maxWidthProperty;

        SerializedProperty useMaxHeightProperty;
        SerializedProperty useMaxWidthProperty;

        protected override void OnEnable()
        {
            base.OnEnable();

            layoutMax = target as LayoutElementWithMaxValuesNew;

            maxHeightProperty = serializedObject.FindProperty(nameof(layoutMax.maxHeight));
            maxWidthProperty = serializedObject.FindProperty(nameof(layoutMax.maxWidth));

            useMaxHeightProperty = serializedObject.FindProperty(nameof(layoutMax.useMaxHeight));
            useMaxWidthProperty = serializedObject.FindProperty(nameof(layoutMax.useMaxWidth));
        }

        public override void OnInspectorGUI()
        {
            Draw(maxWidthProperty, useMaxWidthProperty);
            Draw(maxHeightProperty, useMaxHeightProperty);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }

        void Draw(SerializedProperty property, SerializedProperty useProperty)
        {
            Rect position = EditorGUILayout.GetControlRect();

            GUIContent label = EditorGUI.BeginProperty(position, null, property);

            Rect fieldPosition = EditorGUI.PrefixLabel(position, label);

            Rect toggleRect = fieldPosition;
            toggleRect.width = 16;

            Rect floatFieldRect = fieldPosition;
            floatFieldRect.xMin += 16;

            var use = EditorGUI.Toggle(toggleRect, useProperty.boolValue);
            useProperty.boolValue = use;

            if (use)
            {
                EditorGUIUtility.labelWidth = 4;
                property.floatValue = EditorGUI.FloatField(floatFieldRect, new GUIContent(" "), property.floatValue);
                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUI.EndProperty();
        }
    }
#endif
}

