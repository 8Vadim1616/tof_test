using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class GooglePlayReceiptData
    {
        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("json")]
        public string Data { get; set; }
    }

    public class AppStoreReceiptData
    {
        public string Data { get; set; }
    }

	public class WindowsStoreReceiptData
	{
		public string Data { get; set; }
	}
    
    public class EditorReceiptData
    {
        public string Data { get; set; }
    }

    public class PurchaseReceipt
    {
        [JsonProperty("Store")]
        public string Store { get; set; }

        [JsonProperty("TransactionID")]
        public string TransactionID { get; set; }

        [JsonProperty("Payload")]
        public string Payload { get; set; }

        public GooglePlayReceiptData GetGooglePlayReceiptData()
        {
            if (Payload == null) return null;

            try
            {
                Debug.Log("GetGooglePlayReceiptData: " + Payload);

                var result = JsonConvert.DeserializeObject<GooglePlayReceiptData>(Payload);
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError("Cannot get GooglePlayReceiptData\n" + e);
            }

            return null;
        }

        public AppStoreReceiptData GetAppStoreReceiptData() { return new AppStoreReceiptData() {Data = Payload};}
        public WindowsStoreReceiptData GetWindowsStoreReceiptData() { return new WindowsStoreReceiptData() {Data = Payload};}
    }


}
