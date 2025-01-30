using Assets.Scripts.Events;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Localization;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Static.Bank;
using Assets.Scripts.User.Ad;
using Assets.Scripts.User.BankPacks;
using Assets.Scripts.User.Sockets;
using Assets.Scripts.Utils;
using DefaultNamespace;
using System;
using System.Collections.Generic;
using Assets.Scripts.Network.Queries.Operations.Api.GamePlay;
using Assets.Scripts.Network.Queries.Operations.Api.Social;
using UniRx;
using UnityEngine;
using Assets.Scripts.Static;
using Assets.Scripts.Platform.Mobile.Analytics;
using Assets.Scripts.Static.Ability;
using Assets.Scripts.Static.Levels;
using Assets.Scripts.Static.Tower;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.User.Artifacts;
using Assets.Scripts.User.Units;

namespace Assets.Scripts.User
{
	public class UserData : IDisposable
	{
		public const string DEFAULT_UID = "-1";
		//private const string CONFIRM_TEXT = "confirm";

		
		public UserAbility Ability { get; private set; }

		public UserTower Tower { get; private set; }
		public UserRegisterData RegisterData { get; }
		public UserBank Bank { get; private set; }
		public UserItems Items { get; private set; }
		public UserAds Ads { get; private set; }
		public UserBankPacks BankPacks { get; private set; }
		public UserSettings Settings { get; private set; }
		public UserShop Shop { get; private set; }
		public UserSocketChannels Channels { get; private set; }
		public UserSockets Sockets { get; private set; }
		public UserWindows Windows { get; private set; }
		public UserUnits Units { get; private set; }
		public UserEnergy Energy { get; private set; }
		public UserArtifacts Artifacts { get; private set; }

		public ReactiveProperty<PlayerLevel> Level { get; } = new ();
		public bool WasRegisterFromServer { get; private set; }
		public long RegisterTime { get; private set; }
		public string RegisterVersion { get; private set; }
		public bool IsTester { get; private set; }

		public int Group => StaticData.DEFAULT_GRP;

		private bool _isDisposed;

		private string _firstName;
		private string DefaultFreeName => (Uid.IsNullOrEmpty() || IsDefaultUid) ? "Player" : "Player_" + Uid;
		public string FirstName
        {
            get => _firstName.IsNullOrEmpty() ? DefaultFreeName : _firstName;
            set => _firstName = value;
        }

		public StringReactiveProperty Nick { get; } = new();

        public string Name => string.IsNullOrEmpty(FirstName) ? LastName : FirstName;
		public string LastName { get; set; }
		public string FullName => string.IsNullOrEmpty(LastName) ? FirstName : FirstName + " " + LastName;

		public bool IsDefaultName => _firstName.IsNullOrEmpty(); //FirstName.Equals(DefaultFreeName);

		private long _lastWasOnline = 0;
		public long LastWasOnline
		{
			get
			{
				if (IsCurrent)
					return GameTime.Now;

				return _lastWasOnline;
			}
		}

		public Promise Inited = new Promise();

		public event Action Updated;

		public UserData()
		{
			RegisterData = new UserRegisterData(this);
			RegisterData.InitCurrent();
		}

		public UserData(string uid)
		{
			RegisterData = new UserRegisterData(this, uid);
		}

		public void Init()
		{
			_isDisposed = false;

			if (IsCurrent)
			{
				Channels = new UserSocketChannels(this);
				Sockets = new UserSockets();
				Tower = new UserTower();
				Ability = new UserAbility();
				
				/** JAVA
				Bank = new UserBank();
				Ads = new UserAds();
				BankPacks = new UserBankPacks();
				Settings = new UserSettings();
				Shop = new UserShop(this);
				Windows = new UserWindows();
				*/
				Energy = new UserEnergy();
			}
			
			
			Items = new UserItems(this);
			/** JAVA
			Units = new UserUnits();
			Artifacts = new UserArtifacts();
			*/

			_isDisposed = false;
			Game.ServerDataUpdater.UpdatedUser += OnServerDataUpdate;

			EventController.AddListenerOnce<GameEvents.DeleteDataOnGameReload>(RemoveSubscribeOnReload);
		}

		public IPromise CheckLevelUp()
		{
			if (Level.Value != null && Items.Exp >= Level.Value.ExpToNext.Count && Level.Value.NextLevel != null)
			{
				return Game.QueryManager.RequestPromise(new LevelUpOperation())
						   .Then(r => Game.ServerDataUpdater.Update(r))
						   .Then(r => InfoWindow.Of("Level Up", $"Level up {Level.Value.Id}"))
						   .Then(r => CheckLevelUp());
			}
		
			return Promise.Rejected(null);
		}

		private void OnServerDataUpdate(string uid, ServerUser serverData, bool byServer)
		{
			if (uid != Uid)
				return;

			Update(serverData, byServer);
		}

		public bool CrashLog { get; set; } = true;

		public void OnServerDrop(List<ServerDrop> drops)
		{
			if (drops == null)
			{
				Debug.Log($"Got serv Drops, but drops == null");
				return;
			}

			if (!Game.Instance.IsLoaded.Value || Items == null)
			{
				Debug.Log($"Got serv Drops, but: Game.Instance.IsLoaded - {Game.Instance.IsLoaded.Value}, Items == null - {Items == null}");
				_onLoad += () =>
				{
					Utils.Utils.NextFrame()
					.Then(() =>
					{
						OnServerDrop(drops);
						drops = null;
					});
				};

				return;
			}

			Debug.Log("Got serv Drops: " + drops.Count);
			//
			// foreach (var drop in drops)
			// {
			// 	Game.QueryManager.RequestPromise(new AddServerDropOperation(drop))
			// 		.Then(response =>
			// 		{
			// 			Game.ServerDataUpdater.Update(response);
			// 			OnDrop(response.Drop);
			// 		});
			// }

			void OnDrop(List<ServerDrop> drops)
			{
				if (drops is null || Items is null)
					return;

				foreach (var drop in drops)
				{
					if (drop.IsHidden && drop.NeedAddToUserItems)
					{
						//Просто добавляем, без дропа
						Items.AddItems(drop.Items);
						continue;
					}

					if (drop.IsHidden)
					{
						if (drop.NeedAddToUserItems)
						{
							//Просто добавляем, без дропа
							Items.AddItems(drop.Items);
						}

						//Ничего не делаем. Сервер прислал только для инфы, дропаем сами где-то дальше (например в окне реварда)
						continue;
					}

					Items.AddItems(drop.Items);

					if (Game.Instance.IsLoaded.Value)
						Game.HUD.DropController.DropItems(drop.Items,
							Game.MainCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f)));
				}
			}
		}

		public string Uid => RegisterData?.Uid ?? DEFAULT_UID;
		public bool IsCurrent => Uid == Game.User.Uid;
		public bool IsDefaultUid => Uid == DEFAULT_UID;

		public bool IsFake
		{
			get
			{
				if (int.TryParse(Uid, out var parsed))
					return parsed < 0;

				return false;
			}
		}
		public bool IsOnlineAvailable(bool checkActive)
		{
			if (Game.IsSskError || (checkActive && !Game.IsActive)) // Для платёжки отключаем проверку на свёрнутость
				return false;

			return true;
		}

		private IDisposable onlineSub = null;
		private event Action _onLoad;
		public void OnLoad()
		{
			if (IsCurrent)
				Inited.ResolveOnce();

			onlineSub?.Dispose();
			onlineSub = Observable.Interval(TimeSpan.FromMilliseconds(60000))
								  .Subscribe(_ => SendOnline());

			_onLoad?.Invoke();
		}

		public void SendOnline(bool checkActive = true)
		{
			if (!IsOnlineAvailable(checkActive)) return;

			//todo Game.ServiceProvider.MultiRequest(new OnlineOperation());
		}

		public void Update(ServerUser serverData, bool byServer)
		{
			if (serverData == null) 
				return;
			
			if (serverData.Info != null)
			{
				if (serverData.Info.Register.HasValue)
					WasRegisterFromServer = serverData.Info.Register.Value;

				if (!string.IsNullOrEmpty(serverData.Info.FirstName))
					FirstName = serverData.Info.FirstName;

				if (!string.IsNullOrEmpty(serverData.Info.LastName))
					LastName = serverData.Info.LastName;

				if (!serverData.Info.Nick.IsNullOrEmpty())
					Nick.Value = serverData.Info.Nick;
				else
				{
					if (Nick.Value.IsNullOrEmpty())
						Nick.Value = DefaultFreeName;
				}

				if (serverData.Info.Level.HasValue)
					Level.Value = Game.Static.PlayerLevels.Get(serverData.Info.Level.Value);

				if (serverData.Info.LastWasOnline.HasValue)
					_lastWasOnline = serverData.Info.LastWasOnline.Value;

				Energy?.Update(serverData.Info);
			}

			if (serverData.RegisterTime != 0)
				RegisterTime = serverData.RegisterTime;

			if (!string.IsNullOrEmpty(serverData.RegisterVersion))
				RegisterVersion = serverData.RegisterVersion;

			Items?.Update(serverData.Items, serverData.ItemsDelta);
			Ads?.Update(serverData.Ads, byServer);
			Settings?.Update(serverData.Settings);
			Units?.Update(serverData.Units);
			Artifacts?.Update(serverData.Artifacts);

			if (IsCurrent)
			{
				// if (serverData.Level.HasValue)
				// 	Level.Value = Game.Static.PlayerLevels.Get(serverData.Level.Value);
				
				if (serverData.SocketChannels != null)
				{
					if (Channels == null)
						Channels = new UserSocketChannels(this);

					Channels.Update(serverData.SocketChannels);

					Sockets.Subscribe(Channels.All);
				}

				if (serverData.Tester.HasValue)
					SetTester(serverData.Tester.Value);

				Ads?.UpdateSettings(serverData.AdvertisingPartners);
			}

			if (serverData.Tower != null)
			{
				Tower = serverData.Tower;
			}

			if (serverData.Ability != null)
			{
				Ability = serverData.Ability;
			}
			
			Bank?.Update(serverData.Bank, byServer);

			Updated?.Invoke();
		}

		public void SetTester(bool val)
		{
			if (val != IsTester)
			{
				IsTester = val;
				GameLogger.info("TESTER = " + IsTester);
			}
		}

		public void InvokeUpdateUser()
		{
			Updated?.Invoke();
		}

		private void RemoveSubscribeOnReload(GameEvents.DeleteDataOnGameReload e = null)
		{
			Free(true);
		}

		public void Free(bool forceCurrent = false)
		{
			if (_isDisposed) return;
			if (IsCurrent && !forceCurrent) return;
			_isDisposed = true;

			Sockets?.Dispose(); Sockets = null;
			Settings?.Clear();

			Game.ServerDataUpdater.UpdatedUser -= OnServerDataUpdate;
		}

		public void Dispose()
		{
			Free(true);
		}

		public static Promise SaveUserNameAndAvatarRequest(bool needUpdateFromResponse = true)
		{
			Promise promise = new Promise();

			var user = Game.User;

			var req = new UserNameOperation.Request
			{
				FirstName = user.FirstName,
				LastName = user.LastName,
				Lang = GameLocalization.Locale,
				TimeZoneOffset = -(int) TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes
			};

			Game.QueryManager.RequestPromise(new UserNameOperation(req))
				.Then(resp =>
				{
					if (needUpdateFromResponse)
						Game.ServerDataUpdater.Update(resp);
					promise.ResolveOnce();
				});
			return promise;
		}

		public static void SaveLocale()
		{
			var req = new UserNameOperation.Request
			{
				Lang = GameLocalization.Locale,
				TimeZoneOffset = -(int) TimeZoneInfo.Local.BaseUtcOffset.TotalMinutes
			};

			Game.QueryManager.MultiRequest(new UserNameOperation(req));
		}

		public static bool ValidateConfirmString(string confirm)
		{
			string needConfirm = Game.Settings.GetDeleteConfirmStringForLang(GameLocalization.Locale).ToLower();
			return string.Equals(confirm, needConfirm);
		}

		internal static UserData FromServerUser(ServerUser user)
		{
			if (user?.Info != null)
			{
				var userData = new UserData(user.Info.Uid);
				userData.Init();
				userData.Update(user, false);
				return userData;
			}
			return default;
		}
	}
}