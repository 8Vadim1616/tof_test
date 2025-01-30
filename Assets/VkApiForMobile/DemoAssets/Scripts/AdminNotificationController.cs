using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using com.playGenesis.VkUnityPlugin;
using UnityEngine.SceneManagement;

public class AdminNotificationController : MonoBehaviour {
	public InputField input;
	// Use this for initialization
	public void SendNotificationToAdmin(){

		VKRequest r1 = new VKRequest (){
			url="apps.sendRequest?user_id="+input.text+"&text=Новая викторина Вконтакте бросает тебе вызов! Установи игру прямо сейчас!&type=request&name=test1",
			CallBackFunction=OnAppSendRequest
		};
		VkApi.VkApiInstance.Call (r1);

		//Just for testing subsequent requests

		// VKRequest r2 = new VKRequest (){
		// 	url="apps.sendRequest?user_id="+input.text+"&text=hello_from_vk_plugin2&type=request&name=sayhello2",
		// 	CallBackFunction=OnAppSendRequest
		// };
		
		//VkApi.VkApiInstance.Call (r2);
	}
	void OnAppSendRequest(VKRequest r){
		
		if (r.error!=null){
			if(r.error.error_code == "5"){
				SceneManager.LoadScene ("LoginScene");
			}else
				GlobalErrorHandler.Instance.Notification.Notify(r);
			return;
		} else
		{
		    GlobalErrorHandler.Instance.Notification.Notity(r.response);
		}
	}
	public void Back(){
		SceneManager.LoadScene("StarterScene");
	}
}
