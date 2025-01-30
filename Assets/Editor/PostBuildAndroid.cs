using System;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts.BuildSettings;
using Assets.Scripts.UI.WindowsSystem.Editor;
using Assets.Scripts.Utils;
using UnityEditor;
using UnityEngine;

public class PostBuildAndroid
{
	public static void OnBuildDone(string pathToBuiltProject)
    {
	    UploadToServer(pathToBuiltProject);
	    
		// SendMail();
    }

    public static string GetPathToUpload()
    {
	    if (IsDevBuild)
		    return "develop";

	    if (IsReleaseBuild)
		    return $"{PreBuildScript.GetMajorVersion()}.{PreBuildScript.GetMinorVersion()}/{PlayerSettings.bundleVersion}";

	    return $"{PreBuildScript.GetMajorVersion()}.{PreBuildScript.GetMinorVersion()}";
    }

    private static bool IsReleaseBuild => BuildSettings.IsRelease &&
                                          (EditorUI.IsGooglePlay() && BuildSettings.IsUsingObb
                                           || EditorUI.IsAmazon()
                                           || EditorUI.IsHuawei());

    private static bool IsDevBuild => !PreBuildScript.GetBranch().IsNullOrEmpty();


    private static void UploadToServer(string pathToBuiltProject)
    {
	    // if (pathToBuiltProject.EndsWith(".apk"))
		   //  pathToBuiltProject = Path.GetDirectoryName(pathToBuiltProject);
		   //
	    // Debug.Log($"Auto SFTPUpload to path: {pathToBuiltProject}");
	    //
	    // var fileName = PreBuildScript.GetBuildName();
	    // var settings = SFTPSettings
		   //  .OfBuilds(GetPathToUpload())
		   //  .WithIncludeFiles(new List<string>
		   //  {
			  //   $"{fileName}.apk",
			  //   $"{fileName}.main.obb",
		   //  });
		   //
	    // SFTPUploader.SFTPUploader.UploadToSFTP(pathToBuiltProject, settings);
    }

    private static void SendMail()
    {
	    var fileName = PreBuildScript.GetBuildName();
	    var url = $"http://builds.playme8.net:8080/builds/farm/unity/{GetPathToUpload()}/{fileName}.apk";

	    var major = PreBuildScript.GetMajorVersion();
	    var minor = PreBuildScript.GetMinorVersion();
	    var maintenance = Int32.Parse(PreBuildScript.GetMaintenanceVersion());
	    
	    if(maintenance % 2 == 0)
			maintenance --;
	    
	    var changesUrl = $"https://playgenes.atlassian.net/issues/?jql=project%20%3D%20%22GOL%22%20AND%20fixVersion%20%3D%20%22{major}.{minor}.{maintenance}%22";
	    
	    var title = $"Golden Farm {fileName} is now available!";
	    var body = $"======================\nDownload: {url}\n----------------------\nChangelog: {changesUrl}";
	    
	    MailUtils.SendMail(title, body, IsDevBuild ? MailUtils.DEV_MAILS : MailUtils.MAILS);
    }
}
