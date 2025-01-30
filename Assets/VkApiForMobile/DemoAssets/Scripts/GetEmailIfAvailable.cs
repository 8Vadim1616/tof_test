using System.Collections;
using System.Collections.Generic;
using com.playGenesis.VkUnityPlugin;
using UnityEngine;
using UnityEngine.UI;

public class GetEmailIfAvailable : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GetComponent<Text>().text = VkApi.CurrentToken.email != null? VkApi.CurrentToken.email: "Email is uknown";
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
