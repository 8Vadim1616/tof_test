using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Platform.Mobile.PurchasesExt;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.UI.Utils;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class UnityIAP : AbstractStorePurchases, IStoreListener
    {
        public override string TAG => "[UnityIAP]: ";
        
        private IStoreController _controller;
        private IExtensionProvider _extensions;
        private int _inProcessingCount;
        private readonly Dictionary<string, MobileProduct> _mobileProductsCache = new Dictionary<string, MobileProduct>();
        
        protected override void Processing()
        {
            if (_inProcessingCount <= 0)
                base.Processing();
        }

        protected override void Initialise()
		{
			var allConsumable = UserBank.BankItems
										.Where(b => !b.IsSubscription)
										.Select(b => b.Id).ToList()
										.Distinct();

			var allSubscriptions = UserBank.BankItems
										   .Where(b => b.IsSubscription)
										   .Select(b => b.Id).ToList()
										   .Distinct();

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
			
#if UNITY_WSA
            if (Game.User.Settings.IsWsaPaymentTest)
				builder.Configure<IMicrosoftConfiguration>().useMockBillingSystem = true;
#endif

            foreach (var id in allConsumable)
                builder.AddProduct(id, ProductType.Consumable);
            Debug.Log(TAG + "Added consumables: " + string.Join(", ", allConsumable));

            foreach (var id in allSubscriptions)
                builder.AddProduct(id, ProductType.Subscription);
            Debug.Log(TAG + "Added subscriptions: " + string.Join(", ", allSubscriptions));

            UnityPurchasing.Initialize(this, builder);
        }
        

        protected override void CheckNotProcessed()
        {
            Debug.Log(TAG + "CheckNotProcessed");
            
            StartInitialize().Then
            (() =>
            {
                var allHasReceipt = _controller.products.all
                    .Where(p => p.hasReceipt).ToList();

                if (allHasReceipt.Count > 0)
                {
                    IEnumerable<string> products = allHasReceipt.Where(product => product.definition?.id != null)
                        .Select(product => product.definition.id);
                    Debug.Log(TAG + "allHasReceipt " + String.Join(", ", products));

                    foreach (var product in allHasReceipt)
                        NotProcessed.AddOnce(product.definition.id);

                    Processing();
                }
            });
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
		{
			Debug.Log(TAG + "OnInitialized");
            _controller = controller;
            _extensions = extensions;
            SetInitialized();
        }

        protected override void FetchAdditionalProducts(HashSet<ProductDefinition> needToLoad, Action successCallback, Action<string> failCallback)
        {
            _controller.FetchAdditionalProducts(needToLoad, successCallback, error => failCallback(error.ToString()));
        }

        protected override void InitiatePurchase(string productId)
        {
            _controller.InitiatePurchase(productId);
		}

		public void OnInitializeFailed(InitializationFailureReason error)
		{
			Debug.Log(TAG + "OnInitializeFailed");
			SetInitializeFailed(error.ToString());
		}

		/// <summary>IAP ошибка инициализации с сообщением</summary>
		void IStoreListener.OnInitializeFailed(InitializationFailureReason error, string message)
		{
			Debug.Log(TAG + $"IAP initialize failed: {error}, message: {message}");
			SetInitializeFailed($"{error}, message: {message}");
		}

		/// <summary>
		/// Платеж прошел
		/// </summary>
		public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            Debug.Log(TAG + "ProcessPurchase " + e.purchasedProduct.definition.id);
            StartInitialize().Then
            (() =>
            {
                NotProcessed.AddOnce(e.purchasedProduct.definition.id);
                Processing();
            });
            
            return PurchaseProcessingResult.Pending;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason p)
		{
			if (!product.transactionID.IsNullOrEmpty())
				_controller.ConfirmPendingPurchase(product);
            Handler.RemovePayments(product.definition.id);
            ServerLogs.PaymentErrorFromAne(p + "; last orderId = " + product.definition.id);
            Debug.Log(TAG + "OnPurchaseFailed " + product.definition.id);
			ExecErrorCallback(product.definition.id, new Exception(Enum.GetName(typeof(PurchaseFailureReason), p)));
		}

        protected override List<string> GetRestoredPayments(List<string> productIds)
        {
            var paidProducts = _controller.products.all
                .Where(p => productIds.Contains(p.definition.id) && p.hasReceipt)
                .Select(p => p.definition.id).ToList();
            return paidProducts;
        }

        protected override void ProcessingOne(string productId)
        {
            var product = _controller.products.WithID(productId);

			if (product is null)
			{
				Debug.LogError($"Product with id={productId} not found");
				return;
			}

			var receipt = product.GetPurchaseReceipt();

			if (receipt == null)
				return;

			_inProcessingCount++;

			string signature = null;
            string data = null;
            string transactionID = null;

            if (Application.platform == RuntimePlatform.Android)
            {
                var receiptData = receipt.GetGooglePlayReceiptData();

                if (receiptData == null)
                {
                    // ERROR
                    Debug.LogError(TAG + "receiptData is null for " + product.definition.id + " " + product.transactionID);

                    _inProcessingCount--;
                    _controller.ConfirmPendingPurchase(product);
                    return;
                }

                transactionID = receipt.TransactionID;
                signature = receiptData.Signature;
                data = receiptData.Data;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                var receiptData = receipt.GetAppStoreReceiptData();

                if (receiptData == null)
                {
                    // ERROR
                    Debug.LogError(TAG + "receiptData is null fot " + product.definition.id + " " + product.transactionID);

                    _inProcessingCount--;
                    _controller.ConfirmPendingPurchase(product);
                    return;
                }

                transactionID = receipt.TransactionID;
                signature = null;
                data = receiptData.Data;
            }
            else if (Application.platform == RuntimePlatform.WSAPlayerX86 || Application.platform == RuntimePlatform.WSAPlayerX64)
            {
                var receiptData = receipt.GetWindowsStoreReceiptData();

                if (receiptData == null)
                {
                    // ERROR
                    Debug.LogError(TAG + "receiptData is null fot " + product.definition.id + " " + product.transactionID);

                    _inProcessingCount--;
                    _controller.ConfirmPendingPurchase(product);
                    return;
                }

                transactionID = receipt.TransactionID;
                var sig = Game.SessionKey + Game.User.Uid + transactionID + "asldk234mnb";
                signature = Utils.Utils.Hash(sig);
                data = receiptData.Data;
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                data = receipt.Payload;
            }
            
#if UNITY_EDITOR
            if (product != null)
            {
                var bankItem = Game.User.Bank.GetBankItemById(productId);
                if (bankItem != null && bankItem.IsSubscription)
                {
                    var dataJson = JsonConvert.DeserializeObject<JObject>(data);
                    dataJson["autoRenewing"] = true;
                    data = JsonConvert.SerializeObject(dataJson);
                }
            }
#endif

            string productInfo = null;
            try
            {
                productInfo = JsonConvert.SerializeObject(product.metadata);
            }
            catch (Exception e)
            {
                Debug.LogError(TAG + "Error: " + e);
            }

			Handler.PurchaseSuccessful(productId, signature, data, productInfo, transactionID,
				drop =>
				{
					Debug.Log(TAG + "Server purchase success; productId = " + productId);
					ExecSuccessCallback(productId, drop)
						.Then(() =>
						{
							_controller.ConfirmPendingPurchase(product);
							NotProcessed.Remove(productId);
							Debug.Log($"{TAG}NotProcessed after success {productId} {string.Join(",", NotProcessed)}");
						})
						.Catch(ex =>
						{
							Debug.LogError($"{TAG}PurchaseSuccessful error {productId} {ex}");
						})
						.Finally(() => _inProcessingCount--);
				},
				(e) =>
				{
					Debug.Log($"{TAG}Server error {productId}");
					ExecErrorCallback(productId, e)
						.Finally(() => _inProcessingCount--);
				},
				() =>
				{
					Debug.Log($"{TAG}Server already provided; productId = {productId}");
					ExecErrorCallback(productId)
						.Then(() =>
						{
							_controller.ConfirmPendingPurchase(product);
							NotProcessed.Remove(productId);
							Debug.Log($"{TAG}NotProcessed after already provided {productId} {string.Join(",", NotProcessed)}");
						})
						.Catch(ex =>
						{
							Debug.LogError($"{TAG}PurchaseAlreadyProvided error {productId} {ex}");
						})
						.Finally(() => _inProcessingCount--);
				});
        }

        public override IStoreProduct GetProduct(string productId)
        {
            if (_controller == null || Application.isEditor)
            {
                var bankItem = Game.User.Bank.GetBankItemById(productId);
                return new StoreFakeProduct(productId, "$" + (bankItem.Tier - 0.01f).ToString("F2"));
            }
            
            if (!_mobileProductsCache.ContainsKey(productId))
            {
                var unityProduct = _controller.products.WithID(productId);

                if (unityProduct == null || !unityProduct.availableToPurchase)
                {
                    //Debug.LogError(TAG + "Not Found UnityProduct " + productId);
                    return null;
                }

                _mobileProductsCache[productId] = new MobileProduct(unityProduct);
            }

            return _mobileProductsCache[productId];
        }
    }
}