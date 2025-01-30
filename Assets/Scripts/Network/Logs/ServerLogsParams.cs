using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.Utils;

namespace Assets.Scripts.Network.Logs
{
	public class ServerLogsParams
	{
		public Dictionary<string, object> Params { get; private set; } = new Dictionary<string, object>();

		public ServerLogsParams(Dictionary<string, object> newParams = null)
		{
			if (newParams != null)
				Params = newParams;
		}

		public static ServerLogsParams OfWindow(AbstractWindow win)
		{
			ServerLogsParams result = new ServerLogsParams();
			result.AddWinToParams(win);
			return result;
		}

		public static ServerLogsParams OfData(Dictionary<string, object> obj)
		{
			ServerLogsParams result = new ServerLogsParams();
			result.Params["data"] = obj.ToString();

			return result;
		}

		public ServerLogsParams AddBankItems(UserBankItem[] bankItems)
		{
			List<int> ids = new List<int>();

			if (bankItems == null || bankItems.Length == 0) return this;

			foreach (var bankItem in bankItems)
			{
				ids.Add(bankItem.Id);
			}

			if (ids.Count > 0)
				Params["pos"] = ids;

			return this;
		}

		public ServerLogsParams AddBankTab(int tab)
		{
			Params["tab"] = tab;

			return this;
		}

		public ServerLogsParams AddWinToParams(AbstractWindow win)
		{
			if (!win) return this;
			Params["win"] = win.ClassName;
			return this;
		}

		public ServerLogsParams AddCustomParams(Dictionary<string, object> prm)
		{
			if (prm == null) return this;

			foreach (var kv in prm)
				Params[kv.Key] = kv.Value;

			return this;
		}

		public ServerLogsParams AddNeedBuyRef(string referral)
		{
			if (referral != null)
				Params["ref"] = referral;

			return this;
		}

		public ServerLogsParams AddBankRef(bool byUser = false)
		{
			// if (ServerLogs.LAST_LOG_CLIP)
				// params["bk_ref"] = JSON.stringify(ServerLogs.getLastLogParams());

			AddByUser(byUser);

			return this;
		}

		public ServerLogsParams AddByUser(bool byUser)
		{
			if (byUser)
				Params["user"] = 1;

			return this;
		}

		//
		// public function addAd(adPartner: AbstractMobileAdvertPartner, status: String) : ServerLogsParams
		// {
		// 	if (!Game.getBankController()) return this;
		//
		// 	if(adPartner)
		// 		params["ad"] = adPartner.name;
		// 	params["stat"] = status;
		//
		// 	return this;
		// }
		//
		public static string ItemCountsVectorToString(ItemCount[] items)
		{
			List<string> resArray = new List<string>();

			items.ForEach(process);

			return string.Join("|", resArray);

			void process(ItemCount element, int index)
			{
				resArray.Add(element.Item.Id + ":" + element.Count);
			}
		}

		public static Dictionary<string, object> OfNeedItems(ItemCount[] needItems)
		{
			Dictionary<string, object> obj = new Dictionary<string, object>();
			obj["need"] = ItemCountsVectorToString(needItems);

			return obj;
		}

		public static Dictionary<string, object> OfFor(string modelId)
		{
			var obj = new Dictionary<string, object>();
			obj["for"] = modelId;

			return obj;
		}

		public static Dictionary<string, object> OfTarget(IServerLogsClip serverLogClip)
		{
			if (serverLogClip != null)
			{
				var result = new Dictionary<string, object>();
				result["target"] = serverLogClip.ClassName;

				var logParams = serverLogClip.LogParams.Params;
				if (logParams != null && logParams.ContainsKey("obj"))
					result["obj"] = logParams["obj"];

				return result;
			}

			return null;
		}

		public static Dictionary<string, object> OfUser(bool byUser)
		{
			Dictionary<string, object> obj = new Dictionary<string, object>();
			obj["user"] = Convert.ToInt32(byUser);

			return obj;
		}
		
		public override string ToString()
		{
			if (Params == null || Params.Empty()) return "Empty";
			return string.Join(", " ,Params.Select(kv => kv.Key + " : " + kv.Value));
		}
	}
}