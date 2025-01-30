using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Drops;
using Assets.Scripts.Static.Items;
using Assets.Scripts.User.Ad;
using Assets.Scripts.User.MetaPayments;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.Network.Queries.Operations
{
	public class BaseApiRequest : BaseRequest
	{
		[JsonProperty("seq", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public uint Sequence { get; set; }

		[JsonProperty("uid", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Uid { get; set; }

		[JsonProperty("muid", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Muid { get; set; }

		[JsonProperty("auth_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string AuthKey { get; set; }

		[JsonProperty("action", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Action { get; protected set; }

		[JsonProperty("sn", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string SocialNetwork { get; private set; }

		[JsonProperty("tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long Time { get; private set; }
		
		[JsonProperty("srv_tm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long ServerTime { get; private set; }

		[JsonProperty("grp", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public long Group { get; set; }

		[JsonProperty("ver", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Version { get; set; }

		[JsonProperty("win", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string LastWindow { get; set; }

		[JsonProperty("tp", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int tp;

		[JsonProperty("adp_id", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int adp_id;

		[JsonProperty("stm", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int AdShowTime;

		[JsonProperty("ssk", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? Ssk;

		[JsonProperty("ad_params", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, object> AdvertParams;

		public BaseApiRequest(string action)
		{
			Action = action;
			Version = Application.version;
			if (Game.User != null)
			{
				Uid = Game.User.RegisterData.Uid;
				Muid = Game.User.RegisterData.MobileUid;
				AuthKey = Game.User.RegisterData.AuthKey;
			}

			SocialNetwork = Game.Social.Network;
			Time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			ServerTime = GameTime.ServerTime;
			Ssk = Game.SessionKey;
		}

		public override void PrepareToSend()
		{
			string lastWindow = ServerLogs.GetLastWindow();
			if (!string.IsNullOrEmpty(lastWindow))
				LastWindow = ServerLogs.GetLastWindow();
		}

		/// <summary>
		/// Убираем из запроса лишние данные
		/// </summary>
		public override void SetAsMulti()
		{
			Version = default;
			Uid = default;
			AuthKey = default;
			SocialNetwork = default;
		}

		public void UpdateByAdOptions(AdOptions adOptions = null)
		{
			if (adOptions == null)
				return;

			adp_id = adOptions.Adp_id;
			AdShowTime = adOptions.Stm;
			tp = adOptions.Tp;
			AdvertParams = adOptions.AdvertParams;
		}
	}

	public class BaseApiResponse : BaseResponse
	{
		[JsonProperty("sync", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool? isSync;
        [JsonIgnore]
        public bool IsSyncProfile => isSync.HasValue && isSync.Value;

		
		// [JsonProperty("events", DefaultValueHandling = DefaultValueHandling.Ignore)]
		// public Dictionary<string, JToken> Events { get; set; }

		[JsonProperty("tester", DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonConverter(typeof(BoolConverter))]
		public bool? Tester { get; private set; }

		[JsonProperty("reset_moves", DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonConverter(typeof(BoolConverter))]
		public bool? ResetMoves { get; private set; }

		[JsonProperty("debug_console", DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonConverter(typeof(BoolConverter))]
		public bool? DebugConsole { get; private set; }

		[JsonProperty("user", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public virtual ServerUser User { get; set; }

		[JsonProperty("users", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, ServerUser> Users { get; set; }

		[JsonProperty("drop", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<ServerDrop> Drop { get; set; }

		[JsonProperty("responses", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public BaseApiResponse[] Responses;

		[JsonProperty("message", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string Message;

		[JsonProperty("message_key", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string MessageKey;

		[JsonProperty("gdpr", DefaultValueHandling = DefaultValueHandling.Ignore)]
		[JsonConverter(typeof(BoolConverter))]
		public bool? GDPR;

		[JsonProperty("show_adv", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public UserServerAdvert ServerAdvert;

		[JsonProperty("payment", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public List<ItemCount> Payment;

		[JsonProperty("meta_payments", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, MetaPaymentWrapper> MetaPayments;

		[JsonProperty("admin", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public ServerAdmin Admin { get; set; }
		
		[JsonProperty("profiles", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public Dictionary<string, GetSNUserInfoOperation.Response> Profiles;

		[JsonProperty("need_grp", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public int? NeedGroup;

		[JsonProperty("user_state", DefaultValueHandling = DefaultValueHandling.Ignore)]
		public string NextUserState { get; set; }
		
		public List<ItemCount> GetDropByAction(string action)
		{
			if (Drop == null) return null;

			return Drop.Where(drop => drop.Action.Equals(action))
					   .Where(drop => drop.Items != null)
					   .SelectMany(drop => drop.Items)
					   .ToList();
		}

		public List<ItemCount> GetDrop()
		{
			if (Drop == null) return null;

			return Drop.Where(drop => drop.Items != null).SelectMany(drop => drop.Items)
					   .ToList();
		}

		public List<ItemCount> GetAddedDrop()
		{
			if (Drop == null) return null;

			return Drop.Where(drop => drop.Items != null).SelectMany(drop => drop.Items)
					   .Where(itemCount => itemCount.Count > 0)
					   .ToList();
		}

		public List<ItemCount> GetOnlyShowDrop(bool checkNeedAddToUserItems = true)
		{
			if (Drop == null)
				return null;

			return Drop.Where(drop => drop.Items != null && !drop.IsHidden && (!checkNeedAddToUserItems || !drop.NeedAddToUserItems))
				.SelectMany(drop => drop.Items)
				.ToList();
		}

		//private const int TRY_CHANGE_STATE_MAX_LOOPS = 20;

		// public bool TryChangeState()
		// {
		// 	string currentState = GetUserStateData().State;
		// 	string wasState = currentState;
		//
		// 	for (int i = 0; i <= TRY_CHANGE_STATE_MAX_LOOPS; i++)
		// 	{
		// 		if (i == TRY_CHANGE_STATE_MAX_LOOPS)
		// 		{
		// 			Debug.LogWarning($"User States in potential endless loop! Current state is {GetUserStateData().State}.");
		// 			Debug.LogError("User States in potential endless loop!");
		//
		// 			return true;
		// 		}
		//
		// 		var nextStateMachine = Game.Static?.StaticUserStates?.GetByType(currentState);
		// 		if (nextStateMachine == null)
		// 			break;
		//
		// 		if (nextStateMachine.TryGetNextState(this, out string nextState))
		// 			currentState = nextState;
		// 		else
		// 			break;
		// 	}
		//
		// 	if (currentState != wasState)
		// 	{
		// 		GetUserStateData().State = currentState;
		// 		return true;
		// 	}
		//
		// 	return false;
		// }
    }

	public abstract class BaseApiOperation<TRequest, TResponse> : Operation<TRequest, TResponse>
					where TRequest : BaseApiRequest
					where TResponse : BaseApiResponse
	{
		public override string GetRequestFile()
		{
			return "api";
		}
		
		public override string ToString()
		{
			return String.Format("{0} [{1}] [{2}]", base.ToString(), RequestObject.Sequence, RequestObject.Action);
		}

		internal override void OnResponse(TResponse pResponse)
		{
#if !UNITY_WEBGL
			// if (pResponse.Events != null && pResponse.Events.Any())
			// 	Game.Mobile?.Analytics?.HandleServerEvents(pResponse.Events);
			//
			// if (pResponse.Responses != null)
			// {
			// 	foreach (var resp in pResponse.Responses)
			// 		if (resp != null && resp.Events != null && resp.Events.Any())
			// 			Game.Mobile?.Analytics?.HandleServerEvents(resp.Events);
			// }
#endif

			base.OnResponse(pResponse);
		}
	}
}