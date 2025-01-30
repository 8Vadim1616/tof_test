using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.GameServiceProvider;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Platform.Mobile.Social;
using Assets.Scripts.Static.Items;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.User
{
    public class ServerDataUpdater
	{
		private Promise _lockPaymentPromise;
		private IPromise WaitToUnlockPayment() => _lockPaymentPromise ?? Promise.Resolved();
		
		public event Action<string, ServerUser, bool> UpdatedUser;

		public Promise PaymentPromise = (Promise)Promise.Resolved();

		public void Update(BaseApiResponse response)
		{
			if (Game.User is null || !Game.QueryManager.IsValid)
				return;

			if (response.Version != null)
			{
				var buildVersion = Utils.Utils.GetIntVersion(Application.version);
				var serverVersion = Utils.Utils.GetIntVersion(response.Version);

				if (buildVersion < serverVersion)
				{
					Debug.Log("[Game] app version = " + Application.version);
					Debug.Log("[Game] server version = " + response.Version);
					Game.OnVersionError();
					return;
				}
			}
			
			if (/*checkSSK && */response.Ssk.HasValue && !response.Fstssk.HasValue)
			{
				if (Game.Instance.IsLoaded.Value && Game.User != null && Game.SessionKey.HasValue && Game.SessionKey.Value != response.Ssk.Value)
				{
					Debug.Log($"Game.SessionKey.Value = {Game.SessionKey.Value}; response.Ssk = {response.Ssk.Value}");
					Game.OnSskError();
					return;
				}
			}

			//if() todo: проверка на наличие второго профиля

			Game.User?.Sockets?.StartDelaySubscribing();

			Dictionary<string, ServerUser> allOther = new Dictionary<string, ServerUser>();
			if (response.Users != null)
			{
				foreach (var curUser in response.Users)
				{
					if (curUser.Key == Game.User.Uid)
					{
						if (curUser.Value.Info == null)
							curUser.Value.Info = new ServerUserInfo { Uid = curUser.Key };
						// else if (!response.IsLocal)
						// 	Game.ServiceProvider.RequestPromise(new SaveUserInfoStatsOperation(curUser.Value.Info));

						if (curUser.Value.Info.Uid == null)
							curUser.Value.Info.Uid = curUser.Key;

						UpdatedUser?.Invoke(curUser.Key, curUser.Value, true);
					}
					else
						allOther.Add(curUser.Key, curUser.Value);
				}
			}

			foreach (var otherUser in allOther)
			{
				if (otherUser.Value == null) continue;

				if (otherUser.Value.Info == null)
					otherUser.Value.Info = new ServerUserInfo { Uid = otherUser.Key };

				if (otherUser.Value.Info.Uid == null)
					otherUser.Value.Info.Uid = otherUser.Key;

				UpdatedUser?.Invoke(otherUser.Key, otherUser.Value, !response.IsLocal);
			}

			Game.User?.Sockets?.StartDelaySubscribing();
			Game.User.Ads?.OnServerAd(response.ServerAdvert);

			UpdatedUser?.Invoke(Game.User.Uid, response.User, !response.IsLocal);

			// if (response.DebugConsole.HasValue/* && !BuildSettings.BuildSettings.IsEditor*/)
			// 	Game.Instance.IsDebugConsole = response.DebugConsole.Value;

			if (response.Tester.HasValue)
				Game.User.SetTester(response.Tester.Value);
/*
			if (response.GetRemote()?.Offers != null)				
				Game.User?.Offers?.Update(response.GetRemote().Offers);
*/
			MobileSocialConnector.UpdateAdvertProfile(response);
			CheckServerMessages(response);
			CheckPayment(response);

			Game.User?.Sockets?.StopDelaySubscribing();
		}

		private void CheckServerMessages(BaseApiResponse data)
		{
			if (data == null) 
				return;

			if (data.Message == null && data.MessageKey == null)
				return;

			if (data.Message != null)
			{
				var text = data.Message;
				var args = text.Split(' ').ToList();
				var commandChars = args.Shift();

				if (commandChars[0] == '/') // команда
				{
					string command = commandChars.Remove(0, 1);

					if (args.Count > 0)
					{
						var kav = string.Join(" ", args).Split('\"').ToList();

						args = new List<string>();

						for (int i = 0; i < kav.Count; i++)
						{
							var kavData = kav[i];

							if (kavData.Length > 0)
							{
								if (i % 2 == 0)
								{
									var split = kavData.Split(' ');

									foreach (string d in split)
										args.Add(d);
								}
								else
									args.Add(kavData);
							}
						}
					}

					switch (command)
					{
						case "forcechangeuid":

							if (args.Count > 1)
							{
								var uid = args[0];
								var authKey = args[1];

								OnForceChangeUid(uid, authKey);
							}
							break;
						case "installref":

							if (args.Count > 0)
							{
								var installRef = args[0];

								OnInstallRef(installRef);
							}
							break;
					}

					return;
				}
			}

			if (Game.Instance.IsLoaded.Value)
			{
				var msg = data.Message != null ? data.Message : data.MessageKey.Localize();
				InfoWindow.Of("attention".Localize(), msg);
			}
		}

		private void CheckPayment(BaseApiResponse response)
		{
			if (response.Payment == null)
				return;

			var items = response.Payment;

			Game.User.Items.AddItems(items);

			PaymentPromise = new Promise().WithName("PaymentPromise") as Promise;
			
			var showWindow = true;

			var p = WaitToUnlockPayment();
			// if (showWindow) 
			// 	p = p.Then(ShowYouGotWindow);
			p.Then(Game.Windows.WhenAllClosed)
			 .Then(PaymentPromise.ResolveOnce);
		}
		
		public void LockPaymentPromise()
		{
			UnlockPaymentPromise();
			_lockPaymentPromise = new Promise();
		}

		public void LockPaymentByPromise(IPromise promise)
		{
			UnlockPaymentPromise();
			_lockPaymentPromise = new Promise();
			promise.Then(UnlockPaymentPromise);
		}

		public void UnlockPaymentPromise()
		{
			_lockPaymentPromise?.ResolveOnce();
			_lockPaymentPromise = null;
		}

		public static void OnForceChangeUid(string uid, string authKey)
		{
			Game.User.RegisterData.ChangeUid(uid, authKey);
			GameReloader.Reload(true, true);

			GameLogger.info("forcechangeuid = " + uid + " " + authKey);
		}

		public static void OnInstallRef(string installRef)
		{
#if !UNITY_WEBGL
			Game.Mobile.InstallRef.UpdateRef(installRef);
#endif
		}
	}
}