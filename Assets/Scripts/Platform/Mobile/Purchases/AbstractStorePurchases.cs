using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Events;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public abstract class AbstractStorePurchases
    {
        public abstract string TAG { get; }
        
        protected abstract void FetchAdditionalProducts(HashSet<ProductDefinition> additionalProducts, Action successCallback, Action<string> failCallback);
        protected abstract void InitiatePurchase(string productId);
        protected abstract void Initialise();
        protected abstract List<string> GetRestoredPayments(List<string> productIds);
        protected abstract void ProcessingOne(string productId);
        public abstract IStoreProduct GetProduct(string productId);
        protected abstract void CheckNotProcessed();
        
        protected readonly PurchaseHandler Handler = new PurchaseHandler();
        
        public void OnGameLoad()
        {
            RefreshProducts()
                .Then(Processing)
                .Then(CheckNotProcessed);
        }

        private bool IsInited => _initPromise.IsResolved;
        private bool IsInitedAndRefreshed => IsInited && (_refreshProductsPromise == null || _refreshProductsPromise.IsResolved);
        protected virtual List<string> NotProcessed { get; } = new List<string>();

        private readonly Promise _initPromise = new Promise();
        private bool _initializing = false;
        private Promise _refreshProductsPromise;
        private bool _isRefreshing;

		/// <summary>
		/// ОБЯЗАТЕЛЬНО запускаем метод после инициализации платежной системы
		/// </summary>
		protected void SetInitialized()
        {
            _initializing = false;
            Debug.Log(TAG + "IAP Initialized");
            _initPromise.Resolve();
        }

        /// <summary>
        /// ОБЯЗАТЕЛЬНО запускаем метод после неудачной инициализации платежной системы
        /// </summary>
        protected void SetInitializeFailed(string error)
        {
            _initializing = false;
            Debug.LogWarning(TAG + "IAP init Failed: " + error);
#if UNITY_EDITOR
#else
            Utils.Utils.Wait(1).Then(StartInitialize);
#endif
        }

        /// <summary>
        /// Показывает Лоадер, лочит экран 
        /// </summary>
        /// <returns>возвращает промис, который завершается когда платежи проинициализированы
        /// и выполнилось(или отсутствовало) обновление добавочных позиций</returns>
        public IPromise ShowLoaderAndWaitForInitAndRefresh()
        {
            var result = new Promise();
            const string lockTag = "UnityIAP ShowLoaderAndWaitForInitAndRefresh";

            Promise onInitedAndRefreshed = OnInitedAndRefreshed() as Promise;

#if !UNITY_WEBGL
            //Показываем загрузчик, если еще идет инициализация платежки или её обновление
            if (!Game.Mobile.Purchases.IsInitedAndRefreshed)
            {
                //Game.Locker.Lock(lockTag);
                Game.Loader.Show(OnLoaderClick);
            }
#endif

            //Показываем банк, только когда платёжка стала актуальной
            //OnInitedAndRefreshed()
            onInitedAndRefreshed
                .Then(() =>
                {
					if (result.IsPending)
					{
						Debug.Log(TAG + "OnInitedAndRefreshed resolved");
						result.ResolveOnce();
						Game.Loader.Hide();
					}
                })
                .Catch(
                    err =>
                    {
						if (result.IsPending)
						{
							Debug.Log(TAG + "OnInitedAndRefreshed rejected");
							result.Reject(err);
							Game.Loader.Hide();
						}
                    })
                /*.Finally(() =>
                {
					Game.Locker.Unlock(lockTag);
					Game.Loader.Hide();
                })*/;


			Utils.Utils.Wait(Game.Settings.BankLoadWaitTime)
				.Then(() =>
				{
					if (onInitedAndRefreshed.IsPending)
					{
						Debug.Log(TAG + "OnInitedAndRefreshed rejected after wait");

						onInitedAndRefreshed?.RejectOnce();

						Game.Loader.Hide();
						//NoConnectionWindow.Of();
					}
				});


			void OnLoaderClick()
            {
                onInitedAndRefreshed?.RejectOnce();
                //Game.Locker.Unlock(lockTag);
                //Game.Loader.Hide();
            }

            return result;
        }

        /// <summary>
        /// Завершается когда платежи проинициализированы и выполнилось(или отсутствовало) обновление добавочных позиций
        /// </summary>
        /// <returns></returns>
        private IPromise OnInitedAndRefreshed()
        {
			var result = new Promise();

            // _StartWaitingInitialization();

            _initPromise
                .Then(() =>
                {
                    if (!result.IsPending)
                        return;
                    
                    if (_refreshProductsPromise == null)
                        result.ResolveOnce();
                    else
                        _refreshProductsPromise
                            .Then(result.ResolveOnce)
                            .Catch(result.RejectOnce);
                })
                .Catch(result.RejectOnce);

            return result;

            void _StartWaitingInitialization()
            {
                if (IsInitedAndRefreshed)
                    return;
                
                int delay = Game.User.Settings.PaymentInitTime;
                const int waitingTime = 3;

                if (delay < 0)
                    return;
                if (delay == 0)
                    delay = waitingTime;
                
                Promise.Resolved()
                    .Then(() => Utils.Utils.Wait(delay))
                    .Then(() =>
                    {
                        if (IsInitedAndRefreshed)
                            return;

                        result.RejectOnce();
                        // InfoScreen.Of("attention".Localize(), "payment_not_inited".Localize());
                    });
            }
        }
        
        /// <summary>
        /// Обновить продукты от платформы. Берем всё что есть в UserBank.BankItems
        /// Смотрим на то что уже загружено. Ищем разницу и пытаемся дозагрузить остальное.
        /// </summary>
        public IPromise RefreshProducts()
        {
            _refreshProductsPromise = new Promise();
            StartInitialize().Then(refresh);
            return _refreshProductsPromise;

            void refresh()
            {
                if (_isRefreshing)
                {
                    Debug.Log(TAG + "RefreshProducts is already runed");
                    return;
                }

                _isRefreshing = true;
				var needToLoad = getItemsToLoad();

				if (needToLoad.Count > 0)
                {
                    Debug.Log(TAG + "Try to Fetch additional products");
                    Debug.Log(TAG + string.Join(", ", needToLoad.Select(product => product.id).ToList()));
                
                    FetchAdditionalProducts(needToLoad,
                        () =>
                        {
                            Debug.Log(TAG + "FetchAdditionalProducts complete");
                            _isRefreshing = false;

							foreach (var product in needToLoad)
							{
								if (GetProduct(product.id) == null)
									EventController.TriggerEvent(new GameEvents.SomeProductInintedEvent(product.id));
							}

							if (getItemsToLoad().Count > 0)  // Проверяем еще раз, вдруг за время обновления что-то еще пришло.
							{
								Utils.Utils.Wait(1)
									 .Then(refresh);
							}
							else
							{
								_refreshProductsPromise.ResolveOnce();
							}
						},
						failReason =>
                        {
                            Debug.LogError(TAG + "FetchAdditionalProducts fail " + failReason);
                            _isRefreshing = false;
                            _refreshProductsPromise.RejectOnce();
                        });
                }
                else
                {
                    Debug.Log(TAG + "No need to refresh. Why it's invoked?");
                    _isRefreshing = false;
                    _refreshProductsPromise.ResolveOnce();
                }
            }

			HashSet<ProductDefinition> getItemsToLoad()
			{
				var result = new HashSet<ProductDefinition>();

				foreach (var bankItem in UserBank.BankItems)
				{
					if (GetProduct(bankItem.Id) == null)
					{
						result.Add(new ProductDefinition(bankItem.Id,
														 bankItem.IsSubscription
																		 ? ProductType.Subscription
																		 : ProductType.Consumable));
					}
				}

				return result;
			}
        }

        /// <summary>
        /// Производим платеж. Ждем когда пратежная система полностью проинициализируется
        /// И добавляем платеж в очередь платежей.
        /// </summary>
		public void MakePurchase(UserBankItem bankItem, string referer, Dictionary<string, object> parametres)
		{
			string productId = bankItem.ProductId;
			OnInitedAndRefreshed().Then(() =>
			{
				var product = GetProduct(productId);

				Handler.referer = referer;
				Handler.parametres = parametres;

				if (product != null)
				{
					Handler.SaveOriginalBankId(productId, bankItem.Id);
					Handler.lastGAPosition = productId;
				}

				Debug.Log(TAG + "MakePurchase " + productId);
				InitiatePurchase(productId);
			});
		}

		protected IPromise ExecSuccessCallback(string productId, List<ItemCount> drop)
		{
			return Game.User?.Bank?.ExecSuccessCallback(productId, drop)
				.Then(() => Game.User?.Bank?.EndProccessingProduct(productId)) ?? Promise.Rejected(null);
		}

		protected IPromise ExecErrorCallback(string productId, Exception e = null)
		{
			return Game.User?.Bank?.ExecErrorCallback(productId, e)
				.Then(() => Game.User?.Bank?.EndProccessingProduct(productId)) ?? Promise.Rejected(null);
		}

		public IPromise StartInitialize()
        {
            if (_initializing) return _initPromise;
            if (_initPromise.CurState == PromiseState.Resolved) return _initPromise;
            _initializing = true;
            
            Debug.Log(TAG + "Start initialize");
			
            Initialise();

            return _initPromise;
        }
        
        public IPromise<List<string>> RestorePayments(List<string> productIds)
        {
            Debug.Log(TAG + "RestorePayments : Start");
            return RefreshProducts()
                .Then(CheckNotProcessed)
                .Then(() =>
                {
                    var paidProducts = GetRestoredPayments(productIds);
                    Debug.Log(TAG + "RestorePayments : PaidProducts " + string.Join(", ", paidProducts));
                    return Promise<List<string>>.Resolved(paidProducts);
                });
        }


        protected virtual void Processing()
        {
            if (NotProcessed.Count > 0)
                Debug.Log(TAG + "Processing: " + string.Join(", ", NotProcessed));

			var clone = NotProcessed.Clone();

            foreach (var productId in clone)
                ProcessingOne(productId);
        }
    }
}