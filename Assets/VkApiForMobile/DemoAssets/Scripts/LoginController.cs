using UnityEngine;
using System.Collections;
using com.playGenesis.VkUnityPlugin;
using UnityEngine.SceneManagement;

public class LoginController : MonoBehaviour {
	VkApi vkapi;

	public void Start(){
		vkapi=VkApi.VkApiInstance;
		if(vkapi.IsUserLoggedIn)
			SceneManager.LoadScene("StarterScene");
	}

	public void LoginToVK()
	{
		VkApi.VkSetts.forceOAuth = false;
		vkapi.LoggedIn += onVKLogin;
		vkapi.Login ();
	}
	public void LoginVKOauth()
	{
		vkapi.LoggedIn += onLogin;
		

		VkApi.VkSetts.forceOAuth = true;
		vkapi.Login ();
	}
	public void onLogin(){
		vkapi.LoggedIn -= onLogin;
		SceneManager.LoadScene("StarterScene");
	}
	public void onVKLogin(){
		vkapi.LoggedIn -= onVKLogin;
		var email = VkApi.CurrentToken.email;
		email = email == null ? "no mail":email;
		Debug.Log("email is: "+email);
		SceneManager.LoadScene("StarterScene");
		
	}
}
