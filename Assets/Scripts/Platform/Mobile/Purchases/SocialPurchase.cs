using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Platform.Adapter.Actions;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
	public class SocialPurchase
	{
		private const string TAG = "[SocialPurchase]";

		private Dictionary<string, JToken> _serverPaymentTransactions = new Dictionary<string, JToken>();
		private bool _checkPaymentTransactionsInProcess = false;
		private bool _canCheckTransactions = false;

		public void MakePurchase(UserBankItem bankItem, string referer, Dictionary<string, object> parametres)
		{
			GameLogger.info($"buySocial from {bankItem.SnMoney} SnMoney, id={bankItem.Id}");

			var need = bankItem.SnMoney;

			var otherParams = new JObject();
			otherParams["ref"] = referer;
			otherParams["loc"] = Game.User.Group;
			otherParams["bk_ref"] = JsonConvert.SerializeObject(parametres);

			if (bankItem.Period > 0)
			{
				Game.Social.Request(new Subscription(bankItem.Name, bankItem.Description, bankItem.Position, need,
						otherParams))
					.Then(data =>
					{
						GameLogger.debug("get subscription data");
					})
					.Catch(e =>
					{
						GameLogger.warning(e);
						GameLogger.warning("cancel payment");

						Game.User.Bank.ExecErrorCallback(bankItem.ProductId, e);
					});
			}
			else
			{
				Game.Social.Request(new Payment(bankItem.Name, bankItem.Description, bankItem.Position, need,
						otherParams))
					.Then(data =>
					{
						GameLogger.debug("get bank data");
					})
					.Catch(e =>
					{
						GameLogger.warning(e);
						GameLogger.warning("cancel payment");

						Game.User.Bank.ExecErrorCallback(bankItem.ProductId, e);
					});
			}
		}

		public IPromise OnGameStart()
		{
			_canCheckTransactions = true;
			return CheckPaymentTransactions();
		}

		/**В социалках, по аналогии с мобилками, наш сервер присылает не проведённые транзакции*/
		public void OnServerPaymentTransaction(Dictionary<string, JObject> serverPaymentTransactions)
		{
			foreach (var kv in serverPaymentTransactions)
				_serverPaymentTransactions[kv.Key] = kv.Value;
			CheckPaymentTransactions();
		}

		public IPromise CheckPaymentTransactions()
		{
			if (!Game.Instance.IsLoaded.Value || !_canCheckTransactions)
				return Promise.Resolved();

			var result = new Promise();

			process();

			return result;

			void process()
			{
				if (_checkPaymentTransactionsInProcess)
				{
					result.Resolve();
					return;
				}

				var count = _serverPaymentTransactions.Count;
				if (count <= 0)
				{
					_checkPaymentTransactionsInProcess = false;
					result.Resolve();
					return;
				}

				GameLogger.debug($"{TAG} CheckPaymentTransactions count = {count}");

				_checkPaymentTransactionsInProcess = true;

				var transactionId = _serverPaymentTransactions.Keys.First();
				var data = _serverPaymentTransactions[transactionId];
				var transactionQueueData = Game.Social.Adapter.ParseTransactionQueue(data);
				var posId = transactionQueueData.PositionId;
				var signature = transactionQueueData.Signature;
				var bankItem = Game.User.Bank.GetById(posId);
				var productId = bankItem.ProductId;
				int bankId = bankItem.Id;

				sendToLocalServer()
					.Then(drop => Game.User.Bank.ExecSuccessCallback(productId, drop))
					.Then(() => _checkPaymentTransactionsInProcess = false)
					.Then(process)
					.Catch(_ =>
					{
						_checkPaymentTransactionsInProcess = false;
						result.Resolve();
					});

				IPromise<List<ItemCount>> sendToLocalServer()
				{
					var sendPromise = new Promise<List<ItemCount>>();

					ServerLogs.PaymentBefore(productId, bankId);

					Game.QueryManager.RequestPromise(new PaymentOperation(posId, signature, data.ToString(), null, transactionId))
						.Then(
							response =>
							{
								Game.ServerDataUpdater.Update(response);

								if (string.IsNullOrEmpty(response.Error))
								{
									ServerLogs.PaymentGotGoodAnswerFromLocal(productId);
									sendPromise.Resolve(response.GetDrop());
								}
								else
								{
									ServerLogs.PaymentGotBadAnswerFromLocal(productId);
									sendPromise.Reject(null);
								}
							}
						)
						.Catch(exception =>
						{
							if (exception is NoInternetException)
								Debug.LogWarning($"{TAG} Exception {productId}. {exception.Message}");
							else
								Debug.LogError($"{TAG} Exception {productId}. {exception.Message}");

							sendPromise.Reject(null);
						});

					return sendPromise;
				}
			}
		}
	}
}