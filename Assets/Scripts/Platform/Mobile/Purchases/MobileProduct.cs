using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class MobileProduct : IStoreProduct
    {
        private readonly Product _unityProduct;
        public string Price => _unityProduct?.metadata.localizedPriceString.CheckCurrencies() ?? "";

        public bool HasTrial
        {
            get
            {
                if (_unityProduct == null || _unityProduct.definition.type != ProductType.Subscription)
                    return false;
                
                if (Application.platform == RuntimePlatform.IPhonePlayer)
                    return true;
                
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (_unityProduct != null && _unityProduct.metadata is GoogleProductMetadata googleProductMetadata)
                        return !string.IsNullOrEmpty(googleProductMetadata.freeTrialPeriod);
                }

                return false;
            }
        }

        public string Currency => _unityProduct != null ? _unityProduct.metadata.isoCurrencyCode : "USD";
        public string Description => _unityProduct != null ? _unityProduct.metadata.localizedDescription : "";
        public string Title => _unityProduct != null ? _unityProduct.metadata.localizedTitle : "";

        public MobileProduct(Product unityProduct)
        {
            _unityProduct = unityProduct;
        }
    }
}
