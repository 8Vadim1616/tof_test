using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Assets.Scripts.Platform.Mobile.Analytics.Partners
{
    public interface IAnalyticsPartner
    {
        void Init();
        void StarApp();
        void HandleServerEvent(JToken token);
        void EndApp();

    }
}
