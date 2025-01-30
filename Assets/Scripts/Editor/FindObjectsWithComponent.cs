using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class FindObjectsWithComponent : EditorWindow
{
    private string componentName = "";

    [MenuItem("Tools/Find Objects With Component")]
    public static void ShowWindow()
    {
        GetWindow<FindObjectsWithComponent>("Find Objects With Component");
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Objects With Component", EditorStyles.boldLabel);

        // Поле для ввода имени компонента
        componentName = EditorGUILayout.TextField("Component Name", componentName);

        if (GUILayout.Button("Find"))
        {
            FindObjects();
        }
    }

    private void FindObjects()
    {
        if (string.IsNullOrEmpty(componentName))
        {
            Debug.LogWarning("Please enter a component name.");
            return;
        }

        // Поиск объектов в сцене
        Object[] allObjects = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (GameObject obj in allObjects)
        {
            // Проверка, содержит ли объект указанный компонент
            Component component = obj.GetComponent(componentName);
            if (component != null)
            {
                Debug.Log($"Object '{obj.name}' contains component '{componentName}'.", obj);
            }
        }
    }
}
