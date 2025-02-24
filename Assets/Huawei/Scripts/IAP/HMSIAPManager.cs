﻿#if UNITY_EDITOR || !UNITY_WEBGL && !UNITY_WSA

using HuaweiMobileServices.Base;
using HuaweiMobileServices.IAP;
using HuaweiMobileServices.Utils;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HmsPlugin
{
    public class HMSIAPManager : HMSSingleton<HMSIAPManager>
    {
        private static readonly HMSException IAP_NOT_AVAILABLE = new HMSException("[HMSIAPManager] IAP not available");

        public Action OnCheckIapAvailabilitySuccess { get; set; }
        public Action<HMSException> OnCheckIapAvailabilityFailure { get; set; }

        public Action<IList<ProductInfoResult>> OnObtainProductInfoSuccess { get; set; }
        public Action<HMSException> OnObtainProductInfoFailure { get; set; }

        public Action OnRecoverPurchasesSuccess { get; set; }
        public Action<HMSException> OnRecoverPurchasesFailure { get; set; }

        public Action OnConsumePurchaseSuccess { get; set; }
        public Action<HMSException> OnConsumePurchaseFailure { get; set; }

        public Action<PurchaseResultInfo> OnBuyProductSuccess { get; set; }
        public Action<int> OnBuyProductFailure { get; set; }

        public Action<OwnedPurchasesResult> OnObtainOwnedPurchasesSuccess { get; set; }
        public Action<HMSException> OnObtainOwnedPurchasesFailure { get; set; }

        public Action<OwnedPurchasesResult> OnObtainOwnedPurchaseRecordSuccess { get; set; }
        public Action<HMSException> OnObtainOwnedPurchaseRecordFailure { get; set; }

        public Action<IsSandboxActivatedResult> OnIsSandboxActivatedSuccess { get; set; }
        public Action<HMSException> OnIsSandboxActivatedFailure { get; set; }

        private IIapClient iapClient;
        private bool? iapAvailable = null;
        private List<ProductInfo> productInfoList = new List<ProductInfo>();

        private void Start()
        {
            if (HMSIAPKitSettings.Instance.Settings.GetBool(HMSIAPKitSettings.InitializeOnStart))
                CheckIapAvailability();
        }

        public void CheckIapAvailability()
        {
            iapClient = Iap.GetIapClient();
            ITask<EnvReadyResult> task = iapClient.EnvReady;
            task.AddOnSuccessListener((result) =>
            {
                Debug.Log("[HMSIAPManager] checkIapAvailabity SUCCESS");
                iapAvailable = true;
                OnCheckIapAvailabilitySuccess?.Invoke();
                ObtainProductInfo(HMSIAPProductListSettings.Instance.GetProductIdentifiersByType(HMSIAPProductType.Consumable),
                    HMSIAPProductListSettings.Instance.GetProductIdentifiersByType(HMSIAPProductType.NonConsumable),
                    HMSIAPProductListSettings.Instance.GetProductIdentifiersByType(HMSIAPProductType.Subscription));


            }).AddOnFailureListener((exception) =>
            {
                Debug.LogWarning("[HMSIAPManager]: Error on EnvReady");
                IapApiException iapEx = exception.AsIapApiException();
                iapEx.Status.StartResolutionForResult
                (
                    (intent) =>
                    {
                        Debug.Log("[HMSIAPManager]: Success on iapEx Resolution");
                        OnCheckIapAvailabilitySuccess?.Invoke();
                        ObtainProductInfo(HMSIAPProductListSettings.Instance.GetProductIdentifiersByType(HMSIAPProductType.Consumable),
                            HMSIAPProductListSettings.Instance.GetProductIdentifiersByType(HMSIAPProductType.NonConsumable),
                            HMSIAPProductListSettings.Instance.GetProductIdentifiersByType(HMSIAPProductType.Subscription));
                    },
                    (ex) =>
                    {
                        iapClient = null;
                        iapAvailable = false;

                        Debug.LogError("[HMSIAPManager]: ERROR on StartResolutionForResult: " + ex.WrappedCauseMessage + " " + ex.WrappedExceptionMessage);
                        OnCheckIapAvailabilityFailure?.Invoke(exception);
                    }
                );
            });
        }

        public void ObtainProductInfo(List<string> productIdConsumablesList, List<string> productIdNonConsumablesList, List<string> productIdSubscriptionList)
        {

            if (iapAvailable != true)
            {
                OnObtainProductInfoFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }

            if (!IsNullOrEmpty(productIdConsumablesList))
            {
                ObtainProductInfo(new List<string>(productIdConsumablesList), PriceType.IN_APP_CONSUMABLE);
            }
            if (!IsNullOrEmpty(productIdNonConsumablesList))
            {
                ObtainProductInfo(new List<string>(productIdNonConsumablesList), PriceType.IN_APP_NONCONSUMABLE);
            }
            if (!IsNullOrEmpty(productIdSubscriptionList))
            {
                ObtainProductInfo(new List<string>(productIdSubscriptionList), PriceType.IN_APP_SUBSCRIPTION);
            }
        }

        private void ObtainProductInfo(IList<string> productIdNonConsumablesList, PriceType priceType)
        {

            if (iapAvailable != true)
            {
                OnObtainProductInfoFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }

            ProductInfoReq productInfoReq = new ProductInfoReq
            {
                PriceType = priceType,
                ProductIds = productIdNonConsumablesList
            };

            var wasResponse = false;

            iapClient.ObtainProductInfo(productInfoReq).AddOnSuccessListener((type) =>
            {
                wasResponse = true;
                
                Debug.Log("[HMSIAPManager]:" + type.ErrMsg + type.ReturnCode.ToString());
                Debug.Log("[HMSIAPManager]: {0=Consumable}  {1=Non-Consumable}  {2=Subscription}");
                Debug.Log("[HMSIAPManager]: Found " + type.ProductInfoList.Count + " type of " + priceType.Value + " products");
                foreach (var productInfo in type.ProductInfoList)
                {
                    if (!productInfoList.Exists(c => c.ProductId == productInfo.ProductId))
                        productInfoList.Add(productInfo);
                    Debug.Log("[HMSIAPManager]: ProductId: " + productInfo.ProductId + ", ProductName: " + productInfo.ProductName + ", Price: " + productInfo.Price);
                }

                OnObtainProductInfoSuccess?.Invoke(new List<ProductInfoResult> { type });
            }).AddOnFailureListener((exception) =>
            {
                if(wasResponse)
                    return;
                wasResponse = true;

                Debug.LogError("[HMSIAPManager]: ObtainProductInfo failed. CauseMessage: " + exception.WrappedCauseMessage + ", ExceptionMessage: " + exception.WrappedExceptionMessage);
                OnObtainProductInfoFailure?.Invoke(exception);
            });
        }

        public void ConsumeOwnedPurchases()
        {

            if (iapAvailable != true)
            {
                OnObtainProductInfoFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }

            OwnedPurchasesReq ownedPurchasesReq = new OwnedPurchasesReq();
            ITask<OwnedPurchasesResult> task = iapClient.ObtainOwnedPurchases(ownedPurchasesReq);
            
            var wasResponse = false;
            
            task.AddOnSuccessListener((result) =>
            {
                wasResponse = true;
                
                Debug.Log("[HMSIAPManager] recoverPurchases");
                foreach (var inAppPurchaseData in result.InAppPurchaseDataList)
                {
                    ConsumePurchaseWithPurchaseData(inAppPurchaseData);
                    Debug.Log("[HMSIAPManager] recoverPurchases result> " + result.ReturnCode);
                }

                OnRecoverPurchasesSuccess?.Invoke();

            }).AddOnFailureListener((exception) =>
            {
                if(wasResponse)
                    return;
                wasResponse = true;
                
                Debug.LogError("[HMSIAPManager] ConsumeOwnedPurchases failed. CauseMessage: " + exception.WrappedCauseMessage + ", ExceptionMessage: " + exception.WrappedExceptionMessage);
                OnRecoverPurchasesFailure?.Invoke(exception);
            });
        }

        public void ConsumePurchase(PurchaseResultInfo purchaseResultInfo)
        {
            ConsumePurchaseWithPurchaseData(purchaseResultInfo.InAppPurchaseData);
        }

        public void ConsumePurchaseWithPurchaseData(InAppPurchaseData inAppPurchaseData)
        {
            string purchaseToken = inAppPurchaseData.PurchaseToken;
            ConsumePurchaseWithToken(purchaseToken);
        }

        public void ConsumePurchaseWithToken(string token)
        {

            if (iapAvailable != true)
            {
                OnObtainProductInfoFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }

            ConsumeOwnedPurchaseReq consumeOwnedPurchaseReq = new ConsumeOwnedPurchaseReq
            {
                PurchaseToken = token
            };

            ITask<ConsumeOwnedPurchaseResult> task = iapClient.ConsumeOwnedPurchase(consumeOwnedPurchaseReq);

            var wasResponse = false;
            
            task.AddOnSuccessListener((result) =>
            {
                wasResponse = true;
                Debug.Log("[HMSIAPManager] consumePurchase");
                OnConsumePurchaseSuccess?.Invoke();
            }).AddOnFailureListener((exception) =>
            {
                if(wasResponse)
                    return;
                wasResponse = true;
                
                Debug.LogError("[HMSIAPManager] ConsumePurchaseWithToken failed. CauseMessage: " + exception.WrappedCauseMessage + ", ExceptionMessage: " + exception.WrappedExceptionMessage);
                OnConsumePurchaseFailure?.Invoke(exception);
            });
        }

        public void BuyProduct(string productId, bool consumeAfter = true, string payload = "")
        {
            var productInfo = GetProductInfo(productId);
            if (productInfo != null)
            {
                InternalBuyProduct(productInfo, consumeAfter, payload);
            }
            else
            {
                Debug.LogError($"[HMSIAPManager] Specified: {productId} could not be found in retrieved product list!");
            }
        }

        public void InternalBuyProduct(ProductInfo productInfo, bool consumeAfter = true, string payload = "")
        {
            if (iapAvailable != true)
            {
                OnObtainProductInfoFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }

            PurchaseIntentReq purchaseIntentReq = new PurchaseIntentReq
            {
                PriceType = productInfo.PriceType,
                ProductId = productInfo.ProductId,
                DeveloperPayload = payload
            };

            ITask<PurchaseIntentResult> task = iapClient.CreatePurchaseIntent(purchaseIntentReq);
            task.AddOnSuccessListener((result) =>
            {
                if (result != null)
                {
                    Debug.Log("[HMSIAPManager]:" + result.ErrMsg + result.ReturnCode.ToString());
                    Debug.Log("[HMSIAPManager]: Buying " + purchaseIntentReq.ProductId);
                    Status status = result.Status;
                    status.StartResolutionForResult((androidIntent) =>
                    {
                        PurchaseResultInfo purchaseResultInfo = iapClient.ParsePurchaseResultInfoFromIntent(androidIntent);

                        if (purchaseResultInfo.ReturnCode == OrderStatusCode.ORDER_STATE_SUCCESS)
                        {
                            Debug.Log("[HMSIAPManager] HMSInAppPurchaseData" + purchaseResultInfo.InAppPurchaseData);
                            Debug.Log("[HMSIAPManager] HMSInAppDataSignature" + purchaseResultInfo.InAppDataSignature);
                            OnBuyProductSuccess.Invoke(purchaseResultInfo);
                            if (consumeAfter)
                                ConsumePurchase(purchaseResultInfo);
                        }
                        else
                        {
                            switch (purchaseResultInfo.ReturnCode)
                            {
                                case OrderStatusCode.ORDER_STATE_CANCEL:
                                    Debug.LogWarning("[HMSIAPManager] User cancel payment");
                                    break;

                                case OrderStatusCode.ORDER_STATE_FAILED:
                                    Debug.LogWarning("[HMSIAPManager] order payment failed");
                                    break;

                                case OrderStatusCode.ORDER_PRODUCT_OWNED:
                                    Debug.LogWarning("[HMSIAPManager] Product owned");
                                    break;

                                default:
                                    Debug.LogError("[HMSIAPManager] BuyProduct failed. ReturnCode: " + purchaseResultInfo.ReturnCode + ", ErrorMsg: " + purchaseResultInfo.ErrMsg);
                                    break;
                            }
                            OnBuyProductFailure?.Invoke(purchaseResultInfo.ReturnCode);
                        }

                    }, (exception) =>
                    {
                        Debug.LogError("[HMSIAPManager] startIntent ERROR");
                    });

                }

            }).AddOnFailureListener((exception) =>
            {
                Debug.LogError("[HMSIAPManager]: BuyProduct failed. CauseMessage: " + exception.WrappedCauseMessage + ", ExceptionMessage: " + exception.WrappedExceptionMessage);
            });
        }

        public void ObtainOwnedPurchases()
        {
            if (iapAvailable != true)
            {
                OnObtainProductInfoFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }


            Debug.Log("[HMSIAPManager] ObtainOwnedPurchaseRequest");
            ObtainOwnedPurchases(new OwnedPurchasesReq() { PriceType = PriceType.IN_APP_CONSUMABLE });
            ObtainOwnedPurchases(new OwnedPurchasesReq() { PriceType = PriceType.IN_APP_NONCONSUMABLE });
            ObtainOwnedPurchases(new OwnedPurchasesReq() { PriceType = PriceType.IN_APP_SUBSCRIPTION });
        }

        private void ObtainOwnedPurchases(OwnedPurchasesReq ownedPurchasesReq)
        {
            ITask<OwnedPurchasesResult> task = iapClient.ObtainOwnedPurchases(ownedPurchasesReq);
            task.AddOnSuccessListener((result) =>
            {
                Debug.Log("[HMSIAPManager] ObtainOwnedPurchases");
                foreach (var item in result.InAppPurchaseDataList)
                {
                    Debug.Log("[HMSIAPManager] ProductId: " + item.ProductId + ", ProductName: " + item.ProductName + ", Price: " + item.Price);
                }
                OnObtainOwnedPurchasesSuccess?.Invoke(result);

            }).AddOnFailureListener((exception) =>
            {
                Debug.LogError("[HMSIAPManager]: ObtainOwnedPurchases failed. CauseMessage: " + exception.WrappedCauseMessage + ", ExceptionMessage: " + exception.WrappedExceptionMessage);
                OnObtainProductInfoFailure?.Invoke(exception);
            });
        }

        public void ObtainOwnedPurchaseRecord()
        {
            if (iapAvailable != true)
            {
                OnObtainOwnedPurchaseRecordFailure?.Invoke(IAP_NOT_AVAILABLE);
                return;
            }

            Debug.Log("HMSP: ObtainOwnedPurchaseRecord");
            ObtainOwnedPurchaseRecord(new OwnedPurchasesReq() { PriceType = PriceType.IN_APP_CONSUMABLE });
            ObtainOwnedPurchaseRecord(new OwnedPurchasesReq() { PriceType = PriceType.IN_APP_NONCONSUMABLE });
            ObtainOwnedPurchaseRecord(new OwnedPurchasesReq() { PriceType = PriceType.IN_APP_SUBSCRIPTION });
        }

        private void ObtainOwnedPurchaseRecord(OwnedPurchasesReq req)
        {
            ITask<OwnedPurchasesResult> task = iapClient.ObtainOwnedPurchaseRecord(req);
            task.AddOnSuccessListener((result) =>
            {
                Debug.Log("HMSP: ObtainOwnedPurchaseRecord");
                foreach (var item in result.InAppPurchaseDataList)
                {
                    Debug.Log("[HMSPlugin]: ProductId: " + item.ProductId + ", ProductName: " + item.ProductName + ", Price: " + item.Price);
                }
                OnObtainOwnedPurchaseRecordSuccess?.Invoke(result);

            }).AddOnFailureListener((exception) =>
            {
                Debug.Log("HMSP: Error on ObtainOwnedPurchaseRecord");
                OnObtainOwnedPurchaseRecordFailure?.Invoke(exception);
            });
        }

        public void RestorePurchases(Action<OwnedPurchasesResult> action)
        {
            OnObtainOwnedPurchasesSuccess = (ownedPurchaseResult) =>
            {
                Debug.Log("Return Code: " + ownedPurchaseResult.ReturnCode);
                Debug.Log("InAppPurchaseDataList: " + ownedPurchaseResult.InAppPurchaseDataList.Count);
                Debug.Log("ItemList: " + ownedPurchaseResult.ItemList.Count);

                action.Invoke(ownedPurchaseResult);
            };

            OnObtainOwnedPurchasesFailure = (error) =>
            {
                Debug.LogError("[HMSIAPManager]: RestorePurchasesError failed. CauseMessage: " + error.WrappedCauseMessage + ", ExceptionMessage: " + error.WrappedExceptionMessage);
            };

            ObtainOwnedPurchases();
        }

        public ProductInfo GetProductInfo(string productID)
        {
            return productInfoList.Find(productInfo => productInfo.ProductId == productID);
        }

        public bool IsNullOrEmpty(List<string> array)
        {
            return (array == null || array.Count == 0);
        }

        public void IsSandboxActivated()
        {
            if (iapClient != null)
            {
                var task = iapClient.SandboxActivated;
                task.AddOnSuccessListener((result) =>
                {
                    Debug.Log("[HMSIAPManager]: IsSandboxActivated success!");
                    OnIsSandboxActivatedSuccess?.Invoke(result);
                }).AddOnFailureListener((exception) =>
                {
                    Debug.LogError("[HMSIAPManager]: IsSandboxActivated failed. CauseMessage: " + exception.WrappedCauseMessage + ", ExceptionMessage: " + exception.WrappedExceptionMessage);
                });
            }
            else
            {
                Debug.LogError("[HMSIAPManager]: IsSandboxActivated failed. IAP is not initialized.");
            }
        }
    }
}
#endif