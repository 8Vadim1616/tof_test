using Assets.Scripts.User;
using UnityEditor;
using UnityEngine;

namespace Editor1
{
	public class UIDEditorWindow : EditorWindow
	{
		[MenuItem("Tools/Change UID")]
		private static void ShowWindow() => GetWindow<UIDEditorWindow>().Show();

		private string Uid;
		private string AuthKey;
        
		private void OnGUI()
		{
			Uid ??= PlayerPrefs.GetString(UserRegisterData.UID_KEY, null);
			AuthKey ??= PlayerPrefs.GetString(UserRegisterData.AUTH_KEY_KEY, null);
            
			var newUid = EditorGUILayout.TextField("UID:", Uid);
			if (newUid != Uid)
			{
				Uid = newUid;
				PlayerPrefs.SetString(UserRegisterData.UID_KEY, Uid);
			}
            
			var newAuthKey = EditorGUILayout.TextField("AuthKey:", AuthKey);
			if (newAuthKey != AuthKey)
			{
				AuthKey = newAuthKey;
				PlayerPrefs.SetString(UserRegisterData.AUTH_KEY_KEY, AuthKey);
			}
		}

		//[MenuItem("Tools/Kill UID", false, 51)]
		//static void ClearUID()
		//{
		//	PlayerPrefs.SetString(UserRegisterData.UID_KEY, null);
		//	PlayerPrefs.SetString(UserRegisterData.AUTH_KEY_KEY, null);
		//	PlayerPrefs.SetString(UserRegisterData.MOBILE_UID_KEY, null);
		//	PlayerPrefs.SetString(UserRegisterData.MOBILE_AUTH_KEY_KEY, null);
		//}
	}
}