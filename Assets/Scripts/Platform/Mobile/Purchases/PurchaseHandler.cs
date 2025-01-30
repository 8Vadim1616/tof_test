using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class PurchaseHandler
    {
        public virtual string TAG => "[PurchaseHandler]";
        
        private const string PAYMENTS_PREF = "pays"; 
        
        public string referer = "";
        public Dictionary<string, object> parametres = null;
        public string lastGAPosition = ""; // Возможно нужно только для хуавея

		public void SaveOriginalBankId(string gaPosition, int originalId)
        {
            var payments = Load();
			var match = payments.LastOrDefault(payData => payData.Pos == gaPosition);
			if (match != null)
				match.Id = originalId;
            else
				payments.Add(new PayData {Pos = gaPosition, Id = originalId});
            Debug.Log($"{TAG} Payment add with id {originalId}");
            Save(payments);
            LogPaymentsPref();
        }
        
        public int GetOriginalBankIdByProductIdReverse(string gaPosition)
        {
            var payments = Load();
			Debug.Log($"{TAG} trying get BankId for {gaPosition}. Payments = {string.Join(", ", payments.Select(x => $"[Id = {x.Id}, Pos = {x.Pos}, OrderId = {x.OrderId}]"))}");
			return payments.LastOrDefault(p => p.Pos == gaPosition)?.Id ?? 0;
        }
        
        public void RemovePayments(string gaPosition)
		{
			return;
			
			//Не удаляем. Пусть храниться последнее соответствие
			
            var payments = Load();
			var match = payments.FirstOrDefault(payData => payData.Id != 0 && payData.Pos == gaPosition);
			if (match != null)
				payments.Remove(match);
			Save(payments);
            LogPaymentsPref();
        }
        
        /*
        public void AddOrderIdToPayment(string gaPosition, string orderId)
        {
            var payments = Load();
            var payData = payments.FirstOrDefault(payData => payData.Pos == gaPosition);
            if (payData != null)
                payData.OrderId = orderId;
            Save(payments);
            Debug.Log(UnityIAP.TAG + $"Payment with id {gaPosition} added orderId = {orderId}");
        }
        */

        private List<PayData> Load()
        {
            var payDataString = PlayerPrefs.GetString(PAYMENTS_PREF);
            if (string.IsNullOrEmpty(payDataString))
				return new List<PayData>();
            return JsonConvert.DeserializeObject<List<PayData>>(payDataString);
        }
        
        private void Save(List<PayData> data)
        {
            var payDataString = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(PAYMENTS_PREF, payDataString);
        }

        private void LogPaymentsPref()
        {
            Debug.Log("*** PAYMENTS ***");
            Debug.Log($"{TAG} {PlayerPrefs.GetString(PAYMENTS_PREF)}");
        }

        internal void PurchaseSuccessful(string productId, string signature, string data, string productInfo,
            string transactionId,
            Action<List<ItemCount>> onSuccess,
            Action<Exception> onError,
			Action onAlreadyProvided)
        {
            var senderId = GetOriginalBankIdByProductIdReverse(productId);
            var sendedPosition = Game.User.Bank.GetById(senderId);

            var bankId = senderId;
			Debug.Log($"{TAG} BankId first = {bankId}");
            if (sendedPosition != null)
                bankId = sendedPosition.Id;
			Debug.Log($"{TAG} BankId second = {bankId}");

			ServerLogs.PaymentBefore(productId, bankId);
#if UNITY_EDITOR
			SendLocalRequest();
#else
			CheckPayment();
#endif

			void CheckPayment()
			{
				Game.QueryManager.RequestPromise(new CheckPaymentOperation(bankId, signature, data, productInfo, transactionId))
					.Then(response =>
					{
						if (response.WasAlreadyProvided)
						{
							ServerLogs.PaymentGotAlreadyProvidedFromServer(productId, bankId);
							RemovePayments(productId);
							onAlreadyProvided();
						}
						else if (string.IsNullOrEmpty(response.Error))
						{
							ServerLogs.PaymentGotGoodAnswerFromServer(productId, bankId);
							SendLocalRequest();
						}
						else
						{
							ServerLogs.PaymentGotBadAnswerFromServer(productId, bankId);
							onError(new Exception(response.Error));
						}
					})
					.Catch(exception =>
					{
						if (exception is NoInternetException)
							Debug.LogWarning($"{TAG} Exception {productId}. {exception.Message}");
						else
							Debug.LogError($"{TAG} Exception {productId}. {exception.Message}");

						onError(exception);
					});
			}

			void SendLocalRequest()
			{
				Game.QueryManager.RequestPromise(new PaymentOperation(bankId, signature, data, productInfo, transactionId))
					.Then(response =>
					{
						Game.ServerDataUpdater.Update(response);
						RemovePayments(productId);

						if (string.IsNullOrEmpty(response.Error))
						{
							ServerLogs.PaymentGotGoodAnswerFromLocal(productId);
							onSuccess(response.GetDrop());
						}
						else
						{
							ServerLogs.PaymentGotBadAnswerFromLocal(productId);
							onError(new Exception(response.Error));
						}
					})
					.Catch(exception =>
					{
						if (exception is NoInternetException)
							Debug.LogWarning($"{TAG} Exception {productId}. {exception.Message}");
						else
							Debug.LogError($"{TAG} Exception {productId}. {exception.Message}");

						onError(exception);
					});
			}
        }

		internal void HuaweiPurchaseSuccessful(string productId, string orderID, string purchaseToken,
			Action<List<ItemCount>> onSuccess,
			Action<Exception> onError,
			Action onAlreadyProvided)
		{
			var senderId = GetOriginalBankIdByProductIdReverse(productId);
			var sendedPosition = Game.User.Bank.GetById(senderId);

			var bankId = senderId;
			if (sendedPosition != null)
				bankId = sendedPosition.Id;

			lastGAPosition = productId;

			ServerLogs.PaymentBefore(productId, bankId);

#if UNITY_EDITOR
			sendLocalRequest();
#else
			checkPayment();
#endif

			void checkPayment()
			{
				Game.QueryManager.RequestPromise(new CheckPaymentHuaweiOperation(bankId, orderID, purchaseToken, productId))
					.Then(response =>
					 {
						 if (response.WasAlreadyProvided)
						 {
							 ServerLogs.PaymentGotAlreadyProvidedFromServer(productId, bankId);
							 onAlreadyProvided();
						 }
						 else if (string.IsNullOrEmpty(response.Error))
						 {
							 ServerLogs.PaymentGotGoodAnswerFromServer(productId, bankId);
							 sendLocalRequest();
						 }
						 else
						 {
							 ServerLogs.PaymentGotBadAnswerFromServer(productId, bankId);
							 onError(new Exception(response.Error));
						 }
					 })
					.Catch(exception =>
					 {
						 if (exception is NoInternetException)
							 Debug.LogWarning($"{TAG} Exception {productId}. {exception.Message}");
						 else
							 Debug.LogError($"{TAG} Exception {productId}. {exception.Message}");

						 onError(exception);
					 });
			}

			void sendLocalRequest()
			{
				Game.QueryManager.RequestPromise(new PaymentHuaweiOperation(bankId, orderID, purchaseToken, productId))
					.Then(
						  response=>
						  {
							  Game.ServerDataUpdater.Update(response);
							  RemovePayments(productId);

							  if (string.IsNullOrEmpty(response.Error))
							  {
								  ServerLogs.PaymentGotGoodAnswerFromLocal(productId);
								  onSuccess(response.GetDrop());
							  }
							  else
							  {
								  ServerLogs.PaymentGotBadAnswerFromLocal(productId);
								  onError(new Exception(response.Error));
							  }
						  }
						 )
					.Catch(exception =>
					 {
						 if (exception is NoInternetException)
							 Debug.LogWarning($"{TAG} Exception {productId}. {exception.Message}");
						 else
							 Debug.LogError($"{TAG} Exception {productId}. {exception.Message}");

						 onError(exception);
					 });
			}
		}
    }

    public class PayData
    {
        /// <summary>
        /// id позиции банка
        /// </summary>
        public int Id;
        
        /// <summary>
        /// Позиция из google_play
        /// </summary>
        public string Pos;

        /// <summary>
        /// Id транзакции
        /// </summary>
        /// <returns></returns>
        public string OrderId;
    }
}