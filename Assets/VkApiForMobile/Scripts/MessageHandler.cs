using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using System.Reflection;

namespace com.playGenesis.VkUnityPlugin {

	public class MessageHandler : MonoBehaviour 
	{
		private VkApi vkapi;
		void Awake()
		{
			//hack to prevent stripping of 1251 encoding on ios il2cpp
			var cp1241 = System.Text.Encoding.GetEncoding(1251).EncodingName;

			vkapi = GetComponent<VkApi> ();
		}

		public void ReceiveNewTokenMessage(string message)
		{
			var token=VKToken.ParseSerializeTokenFromNaviteSdk(message);
			vkapi.onReceiveNewToken (token);
		}


		public void AccessDeniedMessage(string errormessage)
		{

			var error=Error.ParseSerializedFromFromNativeSdk (errormessage);
			Debug.Log ("Access Denied " + error.error_msg);
			vkapi.onAccessDenied (error);
		}

		public void NoVkApp(string msg){
			Debug.Log ("No vk app");
			VkApi.VkSetts.forceOAuth = true;
            vkapi.ResetLock();
			vkapi.Login ();
		}

	}
}