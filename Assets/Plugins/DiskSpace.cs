using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class DiskSpace {
    
	#if UNITY_EDITOR
		
	#elif UNITY_ANDROID
		
	#elif UNITY_IPHONE
		[DllImport("__Internal")]
		private static extern long _iDiskSpace_FreeSpece(string path);
	#endif
	
	
	public static long FreeSpece {
		get
		{
#if UNITY_EDITOR
			string dataPath = Application.persistentDataPath;
			System.IO.DriveInfo[] allDrives = System.IO.DriveInfo.GetDrives();
			foreach (var d in allDrives)
			{
				if (!dataPath.StartsWith(d.Name.Substring(0, 2)))
					continue;

				Debug.Log($"Name {d.Name}");
				Debug.Log($"AvailableFreeSpace {d.AvailableFreeSpace}");
				Debug.Log($"TotalFreeSpace {d.TotalFreeSpace}");
				Debug.Log($"TotalSize {d.TotalSize}");

				return d.AvailableFreeSpace;
			}
#elif UNITY_ANDROID
				using(AndroidJavaObject statFs = new AndroidJavaObject( "android.os.StatFs", Application.persistentDataPath))
				{
					//statFs.getBlockSize() * statFs.getAvailableBlocks()
					return (long)statFs.Call<int>("getBlockSize") * (long)statFs.Call<int>("getAvailableBlocks");
				}
#elif UNITY_IPHONE
				return _iDiskSpace_FreeSpece(Application.persistentDataPath);
#endif

			return -1;
		}
	}
}
