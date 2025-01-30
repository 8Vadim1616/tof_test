using System.Linq;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.Network.Queries.Operations.Api;
using Assets.Scripts.Platform.Adapter;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.UI.WindowsSystem;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Platform.Mobile.Social
{
	public class MobileSocialConnector
	{
		/// <summary>Минимальный интервал между попытками загрузки профиля</summary>
		private const float UserReloadDelaySeconds = 10;

		private static int _oldGroup = 1;
		private string _oldUid = null;
		private string _needSn = null;
		private bool _isLocal = false;

		public static bool IsLoading => UserLoadingPromise?.IsPending == true;
		private static Promise UserLoadingPromise;

		public void Connect(string needSn)
		{
			_needSn = needSn;
			_oldUid = Game.User.Uid;
			_isLocal = needSn == SocialNetwork.ODNOKLASSNIKI || needSn == SocialNetwork.MOJMIR;

			ServerLogs.SocialConnect(_needSn);

			UserLoadingPromise = Promise.Resolved()
				   // .Then(() => Game.MapLoaderView.Show())
				   // .Then(() => Game.GameReloader.LockUpdateActive = true)
				   .Then(Game.Social.Adapter.Login)
				   // .Then(() => Game.GameReloader.LockUpdateActive = false)
				   .Then(() => Game.Windows.CloseAllScreensPromise("MobileSocialConnector.Connect"))
				   .Then(GetUserFromSocialNetwork)
				   .Then(CheckSocialUser)
				   .Then(LoadUserInfoFromSocialNetwork)
				   .Then(SaveNameAndAvatarBeforeReload)
				   .Then(() =>
				   {
					   Game.Windows.OpenWindowOnRestart(showConnectedWindow);

					   // Game.MapLoaderView.Hide();
					   GameReloader.Reload(true); // Везде сбрасываем ssk

					   _isLocal = false;
				   })
				   .Catch(ex =>
				   {
					   // Game.MapLoaderView.Hide();
					   Game.Social.Adapter.Logout();

					   _isLocal = false;
				   })
				   as Promise;

			AbstractWindow showConnectedWindow()
			{
				AbstractWindow win;
				
					win = InfoWindow.Of("sn_connect_title".Localize(), "sn_connect_desc".Localize(_needSn.Localize()))
									 .AddButton(ButtonPrefab.GreenSquareButtonWithText, "ok".Localize())
									 .FinishAddingButtons();

				if (win)
				{
					Game.Windows.HolderSetTopOrder();
					win.ClosePromise.Then(Game.Windows.HolderSetBaseOrder);
				}

				return win;
			}
		}

		public void Disconnect()
		{
			// if (Game.Social.Adapter.SN == SocialNetwork.OKMM)
			// 	UserRegisterData.RemoveCodeUser();

			Promise.Resolved()
				   // .Then(() => Game.MapLoaderView.Show())
				   // .Then(() => Game.GameReloader.LockUpdateActive = true)
				   .Then(Game.Social.Adapter.Logout)
				   // .Then(() => Game.GameReloader.LockUpdateActive = false)
				   .Then(() => Game.Windows.CloseAllScreensPromise("MobileSocialConnector.Disconnect"))
				   .Then(() =>
				   {
					   // Game.MapLoaderView.Hide();
					   GameReloader.Reload(true);

					   _isLocal = false;
				   })
				   .Catch(ex =>
				   {
					   // Game.MapLoaderView.Hide();

					   _isLocal = false;
				   });
		}


		private static (string socnet, GetSNUserInfoOperation.Response profile) _serverAdvertProfile;

		public static void UpdateAdvertProfile(BaseApiResponse response)
		{
			if (response.Profiles == null || response.Profiles.Empty())
				return;

			_serverAdvertProfile = (response.Profiles.First().Key, response.Profiles.First().Value);

			CheckAdvertProfile();
		}

		public static IPromise CheckAdvertProfile()
		{
			return Promise.Resolved();
			// if (BuildSettings.BuildSettings.IsEditor)
			// 	return Promise.Resolved();
			//
			// if (!Game.Instance)
			// 	return Promise.Resolved();
			//
			// if (!Game.Instance.IsLoaded.Value)
			// 	return Promise.Resolved();
			//
			// if (_serverAdvertProfile == default)
			// 	return Promise.Resolved();
			//
			// var result = new Promise();
			//
			// if (Game.TutorController !=null && Game.TutorController.IsAnyTutorActive && Game.TutorController.GetFirstActiveTutor != null)
			// {
			// 		var tutorPromise = Game.TutorController.GetFirstActiveTutor.EndPromise;
			// 		tutorPromise
			// 			.Then(() => LoadProfileWindow.Of(_serverAdvertProfile.profile.level, onConnect, onNo));
			// }
			// else if (Game.IsInGame && Game.Instance && Game.Instance.PlayField && Game.Instance.PlayField.TutorController != null
			// 	&& Game.Instance.PlayField.TutorController.IsRuning)
			// {
			// 	result.Resolve();
			// }
			// else
			// 	LoadProfileWindow.Of(_serverAdvertProfile.profile.level, onConnect, onNo);
			//
			//
			// void onConnect()
			// {
			// 	Game.SessionKey = null;
			// 	Game.ServiceProvider
			// 		.RequestPromise(new LinkToSNOperation(Game.User.Uid, Game.User.RegisterData.AdvertisingId, _serverAdvertProfile.socnet))
			// 		.Then(resp =>
			// 		 {
			// 			 Game.MobileServiceProvider?.ClearLogs();
			// 			 Game.User.RegisterData.ChangeUid(resp.uid, resp.auth_key);
			// 			 var loadProfile = new Promise();
			// 			 var loadStatic = new Promise();
			// 			 Debug.Log($"[LinkToSNOperation] response, hasFiles: {(resp.Files != null ? "TRUE" : "FALSE")}");
			// 			 if (resp.Files != null)
			// 			 {
			// 				 foreach (var kv in resp.Files)
			// 				 {
			// 					 if (kv.Key == null)
			// 						 continue;
			//
			// 					 FileResourcesLoader.GetGroup(_oldGroup).SaveFileWithVersion(kv.Key, kv.Value.Data, kv.Value.Ver.Value);
			// 				 }
			//
			// 				 Game.DataLoader.GetStaticDataPromise().Then(() => loadStatic.ResolveOnce());
			// 			 }
			// 			 else
			// 				 loadStatic.ResolveOnce();
			//
			// 			 loadStatic.Then(() =>
			// 			 {
			// 				 Debug.Log($"[LinkToSNOperation] load static done");
			// 				 Game.ServiceProvider.RequestPromise(new LocalSaveServerProfileOperation(resp.Profile, resp.ServerGlobal))
			// 					 .Then(resp => loadProfile.ResolveOnce());
			// 			 });
			//
			// 			 return loadProfile as IPromise;
			// 		 })
			// 		.Then(CheckGDPRWindow)
			// 		.Then(() =>
			// 		 {
			// 			 GameReloader.Reload(true);
			// 		 });
			// 	
			// 	_serverAdvertProfile = default;
			// }
			//
			// void onNo()
			// {
			// 	ServerLogs.SendLog("advert_connect_disable");
			// 	_serverAdvertProfile = default;
			// 	result.Resolve();
			// }
			//
			// void CheckGDPRWindow()
			// {
			// 	GDPRWindow window = Game.Windows.GetOpenWindow<GDPRWindow>();
			// 	if (!window)
			// 		return;
			//
			// 	window.OnAcceptBtnClick(); //Если подгружаем профиль, значит, раньше уже соглашались
			// }
			//
			// return result;
		}

		private IPromise<SocialProfile> LoadUserInfoFromSocialNetwork()
		{
			if (_isLocal)
			{
				var result = new Promise<SocialProfile>();
				result.Resolve(null);
				return result;
			}

			return Game.Social.Adapter.GetProfile();
		}

		private IPromise SaveNameAndAvatarBeforeReload(SocialProfile profile)
		{
			if (profile == null)
				return Promise.Resolved();

			if (Game.User.IsDefaultName)
			{
				Game.User.FirstName = profile.FirstName;
				Game.User.LastName = profile.LastName;
			}
			//Game.User.Avatar = profile.Avatar;

			return UserData.SaveUserNameAndAvatarRequest(needUpdateFromResponse: false);
		}

		private IPromise CheckSocialUser(GetSNUserInfoOperation.Response socialUser)
		{
			return Promise.Resolved();
			// if (_isLocal)
			// 	return Promise.Resolved();
			//
			// if (string.IsNullOrEmpty(socialUser.uid))
			// 	return Promise.Rejected(null);
			//
			// var promise = new Promise();
			//
			// if (socialUser.uid == _oldUid)
			// {
			// 	// OnConnect(); //идет первый запрос с info=1 у меня нет такой привязки в базе и я сразу линкую - отдаваю текущий uid после этого никакие другие запросы уже слать не надо, а сейчас приходит еще один auth
			// 	promise.Resolve();
			// }
			// else
			// {
			// 	ChooseSaveWindow.Of(socialUser, OnChooseLoad, OnChooseLocal, OnCancel);
			// }
			//
			// return promise;
			//
			// void OnChooseLoad()
			// {
			// 	Game.Windows.HolderSetBaseOrder();
			// 	OnConnect();
			// }
			//
			// void OnChooseLocal()
			// {
			// 	Game.Windows.HolderSetBaseOrder();
			// 	promise.RejectOnce(null);
			// }
			//
			// void OnCancel()
			// {
			// 	Game.Windows.HolderSetBaseOrder();
			// 	promise.RejectOnce(null);
			// }
			//
			// void OnConnect()
			// {
			// 	Game.SessionKey = null;
			// 	Game.ServiceProvider
			// 		.RequestPromise(new LinkToSNOperation(_oldUid, Game.Social.Adapter.AccessToken, _needSn))
			// 		.Then(resp =>
			// 		{
			// 			Game.User.RegisterData.ChangeUid(resp.uid, resp.auth_key);
			//
			// 			 var loadStatic = new Promise();
			// 			 Debug.Log($"[LinkToSNOperation] response, hasFiles: {(resp.Files != null ? "TRUE" : "FALSE")}");
			// 			 if (resp.Files != null)
			// 			 {
			// 				 foreach (var kv in resp.Files)
			// 				 {
			// 					 if (kv.Key == null)
			// 						 continue;
			//
			// 					 FileResourcesLoader.GetGroup(_oldGroup).SaveFileWithVersion(kv.Key, kv.Value.Data, kv.Value.Ver.Value);
			// 				 }
			//
			// 				 Game.DataLoader.GetStaticDataPromise().Then(() => loadStatic.ResolveOnce());
			// 			 }
			// 			 else loadStatic.ResolveOnce();
			//
			// 			 loadStatic.Then(() =>
			// 			 {
			// 				 Debug.Log($"[LinkToSNOperation] load static done");
			// 				 Game.ServiceProvider.RequestPromise(new LocalSaveServerProfileOperation(resp.Profile, resp.ServerGlobal))
			// 					 .Then(resp => promise.ResolveOnce());
			// 			 });
			// 		});
			// }
		}

		private Promise<GetSNUserInfoOperation.Response> GetUserFromSocialNetwork()
		{
			var result = new Promise<GetSNUserInfoOperation.Response>();

			if (_isLocal)
			{
				result.Resolve(null);
				return result;
			}

			Game.QueryManager
				.RequestPromise(new GetSNUserInfoOperation(Game.Social.Adapter.AccessToken, _needSn))
				.Then(resp =>
				{
					result.Resolve(resp);
				})
				.Catch(ex => result.Reject(ex));

			return result;
		}

		/*public static void ReloadUserInfo()
		{
			if (Game.Social == null || Game.Social.Adapter == null)
				return;
			
			bool isLogged = Game.Social.Adapter.IsLoggedIn?.Value == true;
			if (!isLogged || IsLoading)
				return;

			var connector = new MobileSocialConnector();
			UserLoadingPromise = connector.LoadUserInfoFromSocialNetwork()
				.Then(connector.SaveAvatar)
				.Then(() => Utils.Utils.Wait(UserReloadDelaySeconds)) as Promise;
		}*/

	}
}