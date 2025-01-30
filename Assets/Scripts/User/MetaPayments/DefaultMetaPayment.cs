using System.Collections.Generic;
using System.Runtime.Serialization;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.User.MetaPayments
{
	public sealed class DefaultMetaPayment : MetaPayment
	{
		[JsonProperty("type")] private string type;
		protected override string Type => type;

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context) =>
			Debug.LogError($"Payment type ({type}) missing in meta payment");

		public override IPromise OnCancel() => Promise.Resolved();
		public override IPromise OnConfirm(List<ItemCount> drop)
		{
			if (drop != null && drop.Count > 0)
				Game.User.Items.AddItems(drop);

			return Promise.Resolved();
		}
	}
}