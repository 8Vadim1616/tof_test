using UnityEngine;
using System.Collections;
using com.playGenesis.VkUnityPlugin;
using UnityEngine.SceneManagement;

public class StarterSceneController : MonoBehaviour {

	public void Start(){
		if (VkApi.VkApiInstance.IsUserLoggedIn) {
			return;
		}else{
			VkApi.VkApiInstance.Login();
		}
	}

	public void TestCaptcha(){
		VKRequest r = new VKRequest ()
		{
			url="captcha.force",
			CallBackFunction=OnCaptchaForse
			
		};
		VkApi.VkApiInstance.Call (r);
	}
	void OnCaptchaForse(VKRequest r)
	{
		if (r.error != null) {
			if(r.error.error_code == "5"){
				SceneManager.LoadScene ("LoginScene");
			}else
				FindObjectOfType<GlobalErrorHandler>().Notification.Notify(r);
			return;
		}
		Debug.Log (r.response);
	}
	public void SendNotificationToAdmin(){
		SceneManager.LoadScene ("NotificationToAdmin");
	}
	public void FriendsGet(){
		SceneManager.LoadScene ("Friends");
	}
	public void ShareScreenShot(){
		SceneManager.LoadScene ("ScreenShotShareDemo");
	}
	public void Logout(){
		VkApi.VkApiInstance.Logout ();
		SceneManager.LoadScene ("LoginScene");
	}


}
