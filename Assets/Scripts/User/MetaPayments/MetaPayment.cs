using System;
using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;

namespace Assets.Scripts.User.MetaPayments
{
	public abstract class MetaPayment
	{
		[JsonIgnore]
		protected abstract string Type { get; }

		[JsonProperty("product_id")]
		public string ProductId { get; protected set; }

		[JsonProperty("bank_id")]
		public int BankId { get; protected set; }

		[JsonIgnore]
		public UserBankItem BankItem => Game.User?.Bank?.GetById(BankId);

		public MetaPayment() { }

		public MetaPayment(UserBankItem bankItem)
		{
			if (bankItem != null)
			{
				ProductId = bankItem.ProductId;
				BankId = bankItem.Id;
			}
		}

		/// <summary>
		/// При внутренней ошибке, чтобы не валидировать платеж, вызываем
		/// Promise.Rejected(new PurchaseTransactionException(ProductId, PurchaseTransactionException.Reason.internalError));
		/// Тогда платеж останется на стадии подтверждения.
		/// Если такие ошибки существуют, правим и выливаем новый билд.
		/// Максимально абстрагируем пользователя от пустой траты денег!!!
		/// </summary>
		public abstract IPromise OnConfirm(List<ItemCount> drop);

		public abstract IPromise OnCancel();

		public static Type GetPaymentType(string jsonType)
		{
			return jsonType switch
			{
				ShopMetaPayment.TYPE => typeof(ShopMetaPayment),

				_ => typeof(DefaultMetaPayment),
			};
		}
	}
}