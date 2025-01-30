#if !UNITY_WEBGL && !UNITY_WSA && !UNITY_STANDALONE && BUILD_HUAWEI
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using HmsPlugin;
using HuaweiMobileServices.IAP;
using HuaweiMobileServices.Utils;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class HuaweiIAP : AbstractStorePurchases
    {
        private int _inProcessingCount;
        
        private readonly Dictionary<string, HuaweiProduct> _mobileProductsCache = new Dictionary<string, HuaweiProduct>();
        
        /// <summary>
        /// Список неподтвержденных покупок
        /// </summary>
        private readonly List<InAppPurchaseData> _productPurchasedList = new List<InAppPurchaseData>();
        protected override List<string> NotProcessed => _productPurchasedList.Select(p => p.ProductId).ToList();

        public override string TAG => "[HuaweiIAP]: ";
        
        protected override void Processing()
        {
            if (_inProcessingCount <= 0)
                base.Processing();
        }

		protected override void Initialise()
        {
            new GameObject("HMSIAPManager").AddComponent<HMSIAPManager>();
            
            HMSIAPManager.Instance.CheckIapAvailability();

            HMSIAPManager.Instance.OnCheckIapAvailabilitySuccess += OnInitialized;
            HMSIAPManager.Instance.OnCheckIapAvailabilityFailure += OnInitializeFailed;
        }

        private void OnInitializeFailed(HMSException error) => SetInitializeFailed(error.Message);
        private void OnInitialized()
        {
            HMSIAPManager.Instance.OnBuyProductSuccess += OnBuyProductSuccess;
            HMSIAPManager.Instance.OnBuyProductFailure += OnBuyProductFailure;
            
            SetInitialized();
        }
        
        protected override void CheckNotProcessed()
        {
            Debug.Log(TAG + "CheckNotProcessed");
            
            StartInitialize().Then(() =>
            {
                HMSIAPManager.Instance.RestorePurchases((restoredProducts) =>
                {
                    Debug.Log(TAG + "allHasReceipt " + String.Join(", ", restoredProducts.InAppPurchaseDataList.Select(p => p.ProductId)));

                    foreach (var purchase in restoredProducts.InAppPurchaseDataList)
                        if(!_productPurchasedList.Exists(p => p.PurchaseToken == purchase.PurchaseToken))
                            _productPurchasedList.Add(purchase);
                    
                    Processing();
                });
            });
        }

        protected override void FetchAdditionalProducts(HashSet<ProductDefinition> needToLoad, Action successCallback, Action<string> failCallback)
        {
            var productIdConsumablesList = new List<string>();

            //todo подписки и многоразовые покупки пока не трогаем. Если их добавлять, OnObtainProductInfoSuccess будет вызываться дважды
            foreach (var definition in needToLoad)
                if(definition.type == ProductType.Consumable)
                    productIdConsumablesList.Add(definition.id);
            
            Debug.Log(TAG + "Try Obtain info consumables: " + string.Join(" ", productIdConsumablesList));

            HMSIAPManager.Instance.ObtainProductInfo(productIdConsumablesList, null, null);
            HMSIAPManager.Instance.OnObtainProductInfoSuccess += OnObtainProductInfoSuccess;
            HMSIAPManager.Instance.OnObtainProductInfoFailure += OnObtainProductInfoFailure;

            void OnObtainProductInfoSuccess(IList<ProductInfoResult> products)
            {
                RemoveListeners();
                Debug.Log(TAG + "OnObtainProductInfoSuccess");
                Debug.Log(TAG + "ProductInfoResult count: " + products.Count);
                foreach (var infoResult in products)
                    Debug.Log($"{TAG}InfoResult: [{infoResult.ReturnCode}] err: {infoResult.ErrMsg} products: {string.Join(" ", infoResult.ProductInfoList.Select(p => p.ProductId))}");
                successCallback();
            }

            void OnObtainProductInfoFailure(HMSException error)
            {
                RemoveListeners();
                Debug.Log(TAG + "OnObtainProductInfoFailure " + error);
                failCallback(error.Message);
            }
            
            void RemoveListeners()
            {
                HMSIAPManager.Instance.OnObtainProductInfoSuccess -= OnObtainProductInfoSuccess;
                HMSIAPManager.Instance.OnObtainProductInfoFailure -= OnObtainProductInfoFailure;
            }
        }

        protected override void InitiatePurchase(string productId)
        {
            Game.Loader.Show();
            
            var payload = new Dictionary<string, object>
            {
                {"uid", Game.User.Uid},
			};

            StartInitialize().Then(() => HMSIAPManager.Instance.BuyProduct(productId, consumeAfter: false, payload: JsonConvert.SerializeObject(payload)));
        }
        
            
        private void OnBuyProductSuccess(PurchaseResultInfo result)
        {
            Game.Loader.Hide();
            Debug.Log(TAG + "OnBuyProductSuccess" + result.InAppDataSignature + " " + result.InAppPurchaseData.ProductId + " " + result.InAppPurchaseData.PurchaseToken);
            if(!_productPurchasedList.Exists(p => result.InAppPurchaseData.PurchaseToken == p.PurchaseToken))
                _productPurchasedList.Add(result.InAppPurchaseData);
            Processing();
        }
                    
        private void OnBuyProductFailure(int productId)
        {
            Game.Loader.Hide();
            Handler.RemovePayments(Handler.lastGAPosition);
            ServerLogs.PaymentErrorFromAne(productId + "; last orderId = " + Handler.lastGAPosition);
            Debug.Log(TAG + "OnPurchaseFailed " + productId);
            
            ExecErrorCallback(Handler.lastGAPosition, new Exception("OnPurchaseFailed"));
        }

        protected override List<string> GetRestoredPayments(List<string> productIds)
        {
            return _productPurchasedList.Select(p => p.ProductId)
                                           .Where(productIds.Contains)
                                           .ToList();
        }

        protected override void ProcessingOne(string productId)
        {
            Debug.Log("ProcessingOne " + productId);
            _inProcessingCount++;

            var purchase = _productPurchasedList.FirstOrDefault(p => p.ProductId == productId);
            if (purchase == null)
            {
                Debug.LogError("Purchase " + productId + " is not active!");
                return;
            }
            
            Handler.HuaweiPurchaseSuccessful(productId, purchase.OrderID, purchase.PurchaseToken,
                drop =>
                {
                    Debug.Log($"{TAG}Server purchase success; productId = {productId}");
                    _inProcessingCount--;

                    HMSIAPManager.Instance.ConsumePurchaseWithPurchaseData(purchase);
                    _productPurchasedList.Remove(purchase);
                    ExecSuccessCallback(productId, drop);
                },
                (e) =>
                {
                    Debug.Log($"{TAG}Server error {productId}");
                    ExecErrorCallback(productId, e);
                },
				() =>
				{
					Debug.Log($"{TAG}Server already provided; productId = {productId}");
					_inProcessingCount--;

					HMSIAPManager.Instance.ConsumePurchaseWithPurchaseData(purchase);
					_productPurchasedList.Remove(purchase);
					ExecErrorCallback(productId);
				});
        }

        public override IStoreProduct GetProduct(string productId)
        {
            if (!_mobileProductsCache.ContainsKey(productId))
            {
                var mobileProduct = HMSIAPManager.Instance.GetProductInfo(productId);
                if (mobileProduct == null)
                {
                    Debug.LogWarning(TAG + "Not Found UnityProduct " + productId);
                    return null;
                }
                _mobileProductsCache[productId] = new HuaweiProduct(mobileProduct);
            }

            return _mobileProductsCache[productId];
        }
    }
}
#endif