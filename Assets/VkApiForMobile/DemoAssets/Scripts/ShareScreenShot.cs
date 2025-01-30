using UnityEngine;
using System.Collections;
using com.playGenesis.VkUnityPlugin;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class ShareScreenShot : MonoBehaviour {
	VkApi vkapi;
	string _filePath;
	byte[] jpegScreenShotBytes;
	// Use this for initialization
	void Start () {
		vkapi = VkApi.VkApiInstance;

		if (!vkapi.IsUserLoggedIn) {
		
			vkapi.Login ();
		}
	}

	public void TakeScreenShot ()
	{

		var imagename="screnshot.jpg";

		_filePath=Path.Combine( Application.persistentDataPath,imagename);
#if !UNITY_EDITOR && (UNITY_ANDROID  || UNITY_IOS)
		ScreenCapture.CaptureScreenshot (imagename);
#else
		ScreenCapture.CaptureScreenshot (_filePath);
#endif

		StartCoroutine (LoadScreenShot());

	}
	IEnumerator LoadScreenShot()
	{
		//wait few seconds to assure that Application.CaptureScreenshot has finished creating screenshot
		yield return new WaitForSeconds(3);
		while (!vkapi.IsUserLoggedIn) {
			yield return null;
		}

        var www =  UnityWebRequestTexture.GetTexture("file:///"+_filePath); //new WWW ("file:///"+_filePath);
        yield return www.SendWebRequest();//www;

		if (string.IsNullOrEmpty (www.error)) {
			Texture2D tex=((DownloadHandlerTexture)www.downloadHandler).texture;//www.texture;
			jpegScreenShotBytes= tex.EncodeToJPG();
			List<ShareImage> imgs=new List<ShareImage>();
			ShareImage screenshot=new ShareImage
								  {
									data=jpegScreenShotBytes,
									imageName="screenshot.jpg",
									imagetype=ImageType.Jpeg
								  };
			imgs.Add(screenshot);


			var vkShare=new VKShare(OnShareFinished,"Hello From VK Api",imgs,"http://u3d.as/8HK");
			vkShare.Share();
			   
		}


	}
	void OnShareFinished(VKRequest resp)
	{
		if (resp.error != null)
		{
			if(resp.error.error_code == "5"){
				SceneManager.LoadScene ("LoginScene");
			}else
				FindObjectOfType<GlobalErrorHandler>().Notification.Notify(resp);
			return;
		}

		Debug.Log("Succesfully finished sharing");

	}
	public void Back(){
		SceneManager.LoadScene ("StarterScene");
	}
	


}
