using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations.Api
{
    public class UserInfoOperation : BaseApiOperation<UserInfoOperation.Request, UserInfoOperation.Response>
    {
        public UserInfoOperation(Dictionary<string, long> versions, StaticData.ModelVersionData modelData)
        {
            SetRequestObject(new Request
			{
				versions = versions,
				ModelData = modelData
			});
        }

        public class Request : BaseApiRequest
        {
            [JsonProperty("versions")]
            public Dictionary<string, long> versions;
            
			[JsonProperty("model_data", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public StaticData.ModelVersionData ModelData;
            
            [JsonProperty("deviceInfo")]
            public Dictionary<string, string> deviceInfo;

            public Request() : base("player.info")
            {
                deviceInfo = new Dictionary<string, string>();
                deviceInfo["Time"] = $"{DateTime.Now:yyyy-MM-dd hh:mm:ss}";
                deviceInfo["OperatingSystem"] = SystemInfo.operatingSystem;
                deviceInfo["OperatingSystemFamily"] = SystemInfo.operatingSystemFamily.ToString();
                deviceInfo["SystemLanguage"] = Application.systemLanguage.ToString();
                deviceInfo["size"] = Screen.width + "x" + Screen.height;
                deviceInfo["Platform"] = Application.platform.ToString();
                deviceInfo["BatteryLevel"] = (SystemInfo.batteryLevel * 100).ToString();
                deviceInfo["DeviceModel"] = SystemInfo.deviceModel;
                deviceInfo["DeviceName"] = SystemInfo.deviceName;
                deviceInfo["ProcessorType"] = SystemInfo.processorType;
                deviceInfo["ProcessorCount"] = SystemInfo.processorCount.ToString();
                deviceInfo["MaxTextureSize"] = SystemInfo.maxTextureSize.ToString();
                deviceInfo["SystemMemorySize"] = SystemInfo.systemMemorySize.ToString();
                deviceInfo["GraphicsDeviceID"] = SystemInfo.graphicsDeviceID.ToString();
                deviceInfo["GraphicsDeviceName"] = SystemInfo.graphicsDeviceName;
                deviceInfo["GraphicsMemorySize"] = SystemInfo.graphicsMemorySize.ToString();
                deviceInfo["InstallerName"] = Application.installerName;
				deviceInfo["DiagonalSize"] = GetDeviceDiagonalSizeInInches().ToString("0.00");
            }
			
			private float GetDeviceDiagonalSizeInInches()
			{
				var dpi = Screen.dpi;
				if (dpi == 0)
					return 0;
				var width = Screen.width / dpi;
				var height = Screen.height / dpi;
				return Mathf.Sqrt(Mathf.Pow(width, 2) + Mathf.Pow(height, 2));
			}
        }

        public class Response : BaseApiResponse
        {
        }
    }
}
