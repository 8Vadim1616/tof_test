using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using com.playGenesis.VkUnityPlugin;
using System.Collections.Generic;

[CustomEditor(typeof(VkSettings))]
public class VkSettingsEditor : UnityEditor.Editor {

	public static string auth_url{ 
		get{
			return PlayerPrefs.GetString("auth_url","");} 

		set{
			PlayerPrefs.SetString("auth_url",value);
		}
	}
	public static string keystore_password{ 
		get{
			return PlayerPrefs.GetString("keystore_password","");} 

		set{
			PlayerPrefs.SetString("keystore_password",value);
		}
	}
	public static string alias_password{ 
		get{
			return PlayerPrefs.GetString("alias_password","");} 

		set{
			PlayerPrefs.SetString("alias_password",value);
		}
	}
	public static string keytool_path{ 
		get{
			return PlayerPrefs.GetString("keytool_path","");} 

		set{
			PlayerPrefs.SetString("keytool_path",value);
		}
	}

	public static string keystore_path{ 
		get{
			return PlayerPrefs.GetString("keystore_path","");} 

		set{
			PlayerPrefs.SetString("keystore_path",value);
		}
	}
	public static string alias{ 
		get{
			return PlayerPrefs.GetString("alias","");} 

		set{
			PlayerPrefs.SetString("alias",value);
		}
	}


	public bool scopeFold = false;
	public bool androidFold = true;
	public VkSettings myScript;
	string token;

	Vector2 scrollPosScope;
	Vector2 scrollPosAndroid;

	public override void OnInspectorGUI()
	{
		myScript = (VkSettings)target;
		//DrawDefaultInspector();
		//EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
		myScript.VkAppId=EditorGUILayout.IntField("Vk App Id",myScript.VkAppId);

		if(myScript.VkAppId==0)
			EditorGUILayout.HelpBox("Plese enter you vk app id",MessageType.Warning);

		EditorGUILayout.HelpBox("Avoids using vk app for auth",MessageType.None);
		myScript.forceOAuth = EditorGUILayout.Toggle(new GUIContent("ForceOauth"),myScript.forceOAuth);

		EditorGUILayout.HelpBox("If checked user will have to confirm scope on each auth",MessageType.None);
		myScript.revoke = EditorGUILayout.Toggle(new GUIContent("Revoke"),myScript.revoke);

		scopeFold = EditorGUILayout.Foldout(scopeFold,"Scope");
		if(scopeFold)
		{
			scrollPosScope = EditorGUILayout.BeginScrollView(scrollPosScope, GUILayout.Width(250),GUILayout.Height(350));
			
			EditorGUILayout.HelpBox("Access to advanced methods for Ads API",MessageType.None);
			myScript.ads = EditorGUILayout.Toggle(new GUIContent("Ads"),myScript.ads);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to audio",MessageType.None);
			myScript.audio=EditorGUILayout.Toggle(new GUIContent("Audio"),myScript.audio);

			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to docs",MessageType.None);
			myScript.docs= EditorGUILayout.Toggle(new GUIContent("Docs"),myScript.docs);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to friends",MessageType.None);
			myScript.friends=EditorGUILayout.Toggle(new GUIContent("Friends"),myScript.friends);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to user communities",MessageType.None);
			myScript.groups=EditorGUILayout.Toggle(new GUIContent("Groups"),myScript.groups);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to advanced methods for messaging",MessageType.None);
			myScript.messages=EditorGUILayout.Toggle(new GUIContent("Messages"),myScript.messages);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("APossibility to make API requests without HTTPS",MessageType.None);
			myScript.nohttps=EditorGUILayout.Toggle(new GUIContent("Nohttps"),myScript.nohttps);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to notes",MessageType.None);
			myScript.notes=EditorGUILayout.Toggle(new GUIContent("Notes"),myScript.notes);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to notifications about answers to the user",MessageType.None);
			myScript.notifications=EditorGUILayout.Toggle(new GUIContent("notifications"),myScript.notifications);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("User allowed to send notifications to him/her (for Flash/iFrame apps)",MessageType.None);
			myScript.notify=EditorGUILayout.Toggle(new GUIContent("Notify"),myScript.notify);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to API at any time (you will receive expires_in = 0 in this case)",MessageType.None);
			myScript.offline=EditorGUILayout.Toggle(new GUIContent("Offline"),myScript.offline);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to wiki pages",MessageType.None);
			myScript.pages=EditorGUILayout.Toggle(new GUIContent("Pages"),myScript.pages);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to photos",MessageType.None);
			myScript.photos=EditorGUILayout.Toggle(new GUIContent("Photos"),myScript.photos);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to statistics of user groups and applications where he/she is an administrator",MessageType.None);
			myScript.stats=EditorGUILayout.Toggle(new GUIContent("Stats"),myScript.stats);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to user status",MessageType.None);
			myScript.status=EditorGUILayout.Toggle(new GUIContent("Status"),myScript.status);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to video",MessageType.None);
			myScript.video=EditorGUILayout.Toggle(new GUIContent("Video"),myScript.video);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to standard and advanced methods for the wall",MessageType.None);
			myScript.wall=EditorGUILayout.Toggle(new GUIContent("Wall"),myScript.wall);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to stories",MessageType.None);
			myScript.stories=EditorGUILayout.Toggle(new GUIContent("Stories"),myScript.stories);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Addition of link to the application in the left menu",MessageType.None);
			myScript.plus256=EditorGUILayout.Toggle(new GUIContent("+256"),myScript.plus256);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to user email",MessageType.None);
			myScript.email=EditorGUILayout.Toggle(new GUIContent("Email"),myScript.email);
			
			EditorGUILayout.Separator();
			EditorGUILayout.HelpBox("Access to market",MessageType.None);
			myScript.market=EditorGUILayout.Toggle(new GUIContent("Market"),myScript.market);
			EditorGUILayout.EndScrollView();
		}
		EditorGUILayout.HelpBox("for example 5.40", MessageType.Info);
		myScript.apiVersion=EditorGUILayout.TextField("api version",myScript.apiVersion);

		EditorGUILayout.HelpBox("To make the plugin work in editor you need click the button \"Connect editor to vk\"," +
			"it will open the web browser and after you confirm you will be redirected to the blanck page, " +
			"copy the url and paste it to the field \"auth url\"",MessageType.Info);
		if(GUILayout.Button("Connect editor to vk"))
		{
			var apiVersion = myScript.apiVersion;
			if (String.IsNullOrEmpty(apiVersion))
			{
				EditorUtility.DisplayDialog("Need api version", "Please specify api version", "ok");
				return;
			}

			if (myScript.VkAppId!=0)
			{
				myScript.generateScope();
				var url="https://oauth.vk.com/authorize?client_id="+myScript.VkAppId +
					"&scope="+string.Join(",",myScript.scope.ToArray())+"&" +
					"redirect_uri=https://oauth.vk.com/blank.html&" +
					"display=popup&" +
					$"v={apiVersion}&" +
					"response_type=token&" +
					"revoke=1";
				url=Uri.EscapeUriString(url);
				EditorUtility.DisplayDialog("Connection to vk","If you change scope you need to reconnect","ok");
				Application.OpenURL(url);
			}else
			{
				EditorUtility.DisplayDialog("Error","Please,enter vk app id","ok");
			}
		}
		auth_url = EditorGUILayout.TextField("auth url", auth_url);

		if (GUILayout.Button("Check if it works"))
		{
			var apiVersion = myScript.apiVersion;
			if (String.IsNullOrEmpty(apiVersion)) {
				EditorUtility.DisplayDialog("Need api version", "Please specify api version", "ok");
				return;
			}
			try
			{
				var token = parseTokenFromString().access_token;
				var s = $"https://api.vk.com/method/users.get?fields=photo_200&v={apiVersion}&access_token=" + token;
				Application.OpenURL(s);
			}
			catch {
				var msg = "To make the plugin work in editor you need click the button \"Connect editor to vk\"," +
						  "it will open the web browser and after you confirm you will be redirected to the blanck page, " +
						   "copy the url and paste it to the field \"auth url\"";
				EditorUtility.DisplayDialog("Invalid url", msg, "OK");
            }
			
				
		}


		androidFold = EditorGUILayout.Foldout(androidFold,"android");
		if(androidFold)
		{
			scrollPosAndroid = EditorGUILayout.BeginScrollView(scrollPosAndroid,GUILayout.Height(350));
			

			keytool_path=EditorGUILayout.TextField("keytool path",keytool_path);
			
			EditorGUILayout.Separator();
			keystore_path=EditorGUILayout.TextField("keystore path",keystore_path);

			EditorGUILayout.Separator();
			keystore_password=EditorGUILayout.TextField("keystore password",keystore_password);
			
			EditorGUILayout.Separator();
			alias=EditorGUILayout.TextField("alias",alias);

			EditorGUILayout.Separator();
			alias_password=EditorGUILayout.TextField("alias password", alias_password);

			if(GUILayout.Button("Get SHA1 Fingerprint"))
			{
				var proc = new System.Diagnostics.Process
            	{
					StartInfo = new System.Diagnostics.ProcessStartInfo
					{
						FileName = keytool_path,
						Arguments = "-list -v -keystore " + "\"" + keystore_path + "\"" +
						" -alias " + alias +
						" -storepass " + keystore_password +
						" -keypass " + alias_password,
						UseShellExecute = false,
						RedirectStandardOutput = true,
						RedirectStandardInput = true,
						CreateNoWindow = true
					}
				};
				proc.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(OutputHandler);
				
				using(proc){
					proc.Start();
					proc.BeginOutputReadLine();
					proc.WaitForExit();
				}
					
			}

			EditorGUILayout.EndScrollView();
		}

		if(GUI.changed)
		{
			EditorUtility.SetDirty(myScript);
		}
	}

	private void OutputHandler(object sender, System.Diagnostics.DataReceivedEventArgs e)
    {
		if(e.Data.Contains("\t SHA1: ")){
			var sha1 = e.Data.Replace("\t SHA1: ","");
			sha1 = sha1.Replace(":","");
			UnityEngine.Debug.Log("sha1 fingerprint is: "+ sha1);
			
		}
    }
	private VKToken parseTokenFromString()
	{
		var authUrl=auth_url;
		string[] firstsplit=authUrl.Split('#');
		string[] secondsplit=firstsplit[1].Split('&');
		
		var tokeninfo = new Dictionary<string,string> ();
		
		foreach (var secondsplitemevent in secondsplit)
		{
			string[] thirdsplit=secondsplitemevent.Split('=');
			tokeninfo.Add(thirdsplit[0],thirdsplit[1]);
		}
		VKToken ti1=new VKToken();
		
		int outvar = 99999999;
		ti1.access_token = tokeninfo ["access_token"];
		ti1.expires_in = int.TryParse (tokeninfo ["expires_in"], out outvar) ? outvar : outvar;
		if(outvar==0)
		{
			ti1.expires_in=9999999;
		}
		ti1.user_id = tokeninfo ["user_id"];
		ti1.tokenRecievedTime = DateTime.Now;
		
		return ti1;
	
	}

}
