using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Assets.Scripts.Platform.Mobile.Analytics.Partners
{
    public class AppsFlyerSettings
    {
        [JsonProperty("dev_key")]
        public string DevKey { get; set; }

        [JsonProperty("app_d")]
        public string AppId { get; set; }
    }
}
