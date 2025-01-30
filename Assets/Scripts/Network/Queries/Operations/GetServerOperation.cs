using System;
using System.Collections.Generic;
using Assets.Scripts.Platform.Mobile.Analytics;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations
{
	public class GetServerOperation : BaseApiOperation<GetServerOperation.Request, GetServerOperation.Response>
	{
		public override bool NeedShowWindowError => true;

		public GetServerOperation()
		{
			SetRequestObject(new Request { });
		}

		public class Request : BaseApiRequest
		{
			public Dictionary<string, string> deviceInfo;

			public Request() : base("server.config")
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
			}
		}

		public class Response : BaseApiResponse
		{
			public string server;
			public int timeout;
			public int attempts;
			public int crashlog_tm;
			public string fb_og_url;
			public string fb_invite_url;
			public string partsurl;
			public string addressables;
			public string[] preload_catalogs;
			public int fps;
			public int ping_int;
			public bool? consent;
			public bool? is_test_suite;
			
			[JsonProperty(MobileAnalytics.APPS_FLAYER_ANALYTICS_PARTNER)]
			public bool? IsAppsFlyerAnalyticsAvailable;
			
			[JsonProperty(MobileAnalytics.FIREBASE_ANALYTICS_PARTNER)]
			public bool? IsFirebaseAnalyticsAvailable;
			
			[JsonProperty(MobileAnalytics.FACEBOOK_ANALYTICS_PARTNER)]
			public bool? IsFacebookAnalyticsAvailable;

			[JsonProperty("wait_model", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public int? Wait_Model { get; set; }
			
			[JsonProperty("langs", DefaultValueHandling = DefaultValueHandling.Ignore)]
			public string[] Locales { get; set; }

			[JsonProperty("fb_delete", DefaultValueHandling = DefaultValueHandling.Ignore)]
			[JsonConverter(typeof(BoolConverter))]
			public bool NeedFbDelete;

			[JsonProperty("grp")]
			public int? Group { get; set; }

			public WebSocketSettings wss;

			public string crash_url;
			public int max_crash_for_session;
		}

		public class WebSocketSettings
		{
			public string host;
			public int port;
			public string vhost;
		}
	}
}