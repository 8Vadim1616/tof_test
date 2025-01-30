using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Platform.Mobile.Purchases;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Platform.Mobile.PurchasesExt
{
    public static class UnityIAPExt
    {
        public static PurchaseReceipt GetPurchaseReceipt(this Product product)
        {
            if (product.receipt == null)
            {
                Debug.Log($"GetPurchaseReceipt: PurchaseReceipt for {product.definition.id} is null");
                return null;
            }

            Debug.Log("GetPurchaseReceipt: " + product.receipt);

            return JsonConvert.DeserializeObject<PurchaseReceipt>(product.receipt);
        }
    }
}
