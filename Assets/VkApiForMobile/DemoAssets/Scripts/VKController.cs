using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using com.playGenesis.VkUnityPlugin;
using com.playGenesis.VkUnityPlugin.MiniJSON;


public class VKController : MonoBehaviour {

	// Use this for initialization
	void Start () {
		if (VkApi.VkApiInstance.IsUserLoggedIn) {
			 GetFriendInfo();
		} else {
			VkApi.VkApiInstance.LoggedIn+=onLoggedIn;
			VkApi.VkApiInstance.Login ();
		}
	}
	public void onLoggedIn(){
		VkApi.VkApiInstance.LoggedIn-=onLoggedIn;
		GetFriendInfo();
	}

	public void GetFriendInfo(){
		VKRequest r = new VKRequest
		{
			url="users.get?user_ids=205387401&photo_50",
			CallBackFunction=OnGotUserInfo
		};
		VkApi.VkApiInstance.Call (r);
	}
	public void OnGotUserInfo (VKRequest r)
	{
		if(r.error!=null)
		{
			if(r.error.error_code == "5"){
				SceneManager.LoadScene ("LoginScene");
			}else
				FindObjectOfType<GlobalErrorHandler>().Notification.Notify(r);
			//hande error here
			Debug.Log(r.error.error_msg);
			return;
		}

		//now we need to deserialize response in json from vk server
		var dict=Json.Deserialize(r.response) as Dictionary<string,object>;
		var users=(List<object>)dict["response"];
		var vk_users = VKUser.Deserialize(users.ToArray());
	
		for (int i = 0; i<vk_users.Count;i++){
			

			Debug.Log ("user id is " + vk_users[i].id);
			Debug.Log ("user name is " + vk_users[i].first_name);
			Debug.Log ("user last name is " + vk_users[i].last_name);
		}
	}

	#pragma warning disable 219
	public void ApiCheatSheet(){
		//get vkapi instance object
		var vkapi = VkApi.VkApiInstance;
		//check if user is logged in
		var isLoggedIn = vkapi.IsUserLoggedIn;
		
		//starts login
		vkapi.Login();
		
		//get vk setting the same you can edit selecting menu VK->Edit Vk Setting
		var settings = VkApi.VkSetts;

		//forces usage of webview during login
		settings.forceOAuth = false;

		//forces showing to user required permissions during login
		//if false shows them only the first time you login
		settings.revoke = true;

		//get current token data
		var tokenData = VkApi.CurrentToken;
		//get seconds to token expiration
		var tokenValidForSeconds = tokenData.TokenValidFor(); 
		//check if token has expired
		var isValid = VKToken.IsTokenNotExpired(tokenData);

		//register for authorization access denied event
		vkapi.AccessDenied += (sender, error)=>{ 
			Debug.Log(error.error_msg);
		};

		//register for logged in event
		vkapi.LoggedIn += ()=>{
			Debug.Log("loged in!!");
		};

		//register for logged out event
		vkapi.LoggedIn += ()=>{
			Debug.Log("loged out!!");
		};

		//register for recieved new token event
		vkapi.ReceivedNewToken += (sender, token)=>{
			Debug.Log(token.access_token);
		};

		//new request
		VKRequest r = new VKRequest
		{
			url="users.get?user_ids=205387401&photo_50",
			CallBackFunction = (request) =>{
				Debug.Log(request.response);
			}
		};
		//execute request
		vkapi.Call (r);
	}
	#pragma warning restore 219

}
