using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.playGenesis.VkUnityPlugin {
	public class VkEditorLogin : IVkNativeLogin {
		public void Login()
		{
			var currentToken = VkApi.CurrentToken;
			var VkSetts = VkApi.VkSetts;
			VkSetts.ProcessAuthUrl ();
			String fakeSerializedResponseFormSdk=currentToken.access_token+"#"+currentToken.expires_in+"#"+currentToken.user_id;

			GameObject.FindObjectOfType<MessageHandler>().ReceiveNewTokenMessage(fakeSerializedResponseFormSdk);	
		}
		public void Logout()
		{
			VkApi.VkApiInstance.onLoggedOut ();
		}
	}
}