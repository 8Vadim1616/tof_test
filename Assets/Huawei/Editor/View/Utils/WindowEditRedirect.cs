using UnityEditor;

namespace HmsPlugin
{
    //[CustomEditor(typeof(SettingsScriptableObject))]
    public class SettingsScriptableObjectDrawer : WindowEditRedirect
    {
    }

    public class WindowEditRedirect : CustomEditorDrawer
    {
        private void OnEnable()
        {
#if BUILD_HUAWEI
            _drawer.AddDrawer(new Label.Label("All modifications are performed via the plugin window."));
            _drawer.AddDrawer(new Button.Button("Open Window", HMSMainWindow.ShowWindow));
#endif
        }
    }
}
