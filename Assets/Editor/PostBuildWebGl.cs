using System;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public class PostBuildWebGl
{
    public static void OnBuildDone(string pathToBuiltProject)
    {
		UpdateTimeStamp(pathToBuiltProject);
		
		Debug.Log("Auto SFTPUpload");
		//SFTPUploader.SFTPUploader.UploadToSFTP(pathToBuiltProject, SFTPSettings.OfWebGL());

		// SendMail();
	}

	private static void UpdateTimeStamp(string buildPath)
	{
		Debug.Log("Build path " + buildPath);
		string fileName = "build.json";
		string filePath = Path.Combine(buildPath, fileName);
		FileInfo fi = new FileInfo(filePath);

		if (fi.Exists)
			fi.Delete();

		var data = new JObject();
		data["tm"] = DateTimeOffset.Now.ToUnixTimeSeconds();
		data["ver"] = PlayerSettings.bundleVersion;

		using (FileStream fs = fi.Create())
		{
			byte[] txt = new UTF8Encoding(true).GetBytes(data.ToString());
			fs.Write(txt, 0, txt.Length);
		}
	}

	private static void SendMail()
	{
		var url = "https://vk.com/app5592343_134876812";

		var major = PreBuildScript.GetMajorVersion();
		var minor = PreBuildScript.GetMinorVersion();
		var maintenance = Int32.Parse(PreBuildScript.GetMaintenanceVersion());

		if(maintenance % 2 == 0)
			maintenance --;

		var changesUrl = $"https://playgenes.atlassian.net/issues/?jql=project%20%3D%20%22GOL%22%20AND%20fixVersion%20%3D%20%22{major}.{minor}.{maintenance}%22";

		var title = $"Golden Farm TEST VK {major}.{minor}.{maintenance} is now available!";
		var body = $"======================\nPlay: {url}\n----------------------\nChangelog: {changesUrl}";

		MailUtils.SendMail(title, body, MailUtils.MAILS);
	}
}
