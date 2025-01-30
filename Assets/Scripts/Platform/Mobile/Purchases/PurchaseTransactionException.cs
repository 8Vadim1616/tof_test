using System;

namespace Platform.Mobile.Purchases
{
	public class PurchaseTransactionException : Exception
	{
		public enum Reason
		{
			metaPaymentIsEmpty,
			internalError,
		}

		public PurchaseTransactionException(string productId, Reason reason) : base($"Purchase {productId} drop with error: {reason}")
		{
		}
	}
}