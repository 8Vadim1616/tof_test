using System;
using System.Collections.Generic;
using System.Linq;
using AFMiniJSON;
using Assets.Scripts.Libraries.RSG;
using com.playGenesis.VkUnityPlugin;
using UnityEngine;

namespace Assets.Scripts.Platform.Adapter.Implements
{
	public class VKMobileAdapter : AbstractSocialAdapter
	{
		private static GameObject VKGameObject = null;

		private IMultiAdapter _mobileAdapter;
		private string _fields = "first_name,last_name,sex,photo_medium_rec,photo_rec,photo_big,bdate";

		protected override string TAG => "[VKMobileAdapter] ";
		public override string SN => SocialNetwork.VKONTAKTE;
		public override bool IsMobile => true;

		public override string AccessToken => VkApi.VkApiInstance.IsUserLoggedIn ? VkApi.CurrentToken.access_token : null;

		public override int MaxCountFriendsFromNetwork => 500;

		public VKMobileAdapter(SocialAdapterParams parameters, IMultiAdapter mobileAdapter) : base(SocialNetwork.VKONTAKTE, parameters)
		{
			_mobileAdapter = mobileAdapter;

			Init().Then(AfterInit);
		}

		private Promise Init()
		{
			var vkPromise = new Promise();

			if (VKGameObject == null)
			{
				var socialNetwork = _params.SocialNetwork;

				VKGameObject = GameObject.Instantiate(socialNetwork.VkApiGameObject, Vector3.zero, Quaternion.identity);
				VKGameObject.name = "VkApi";
				VKGameObject.transform.SetParent(socialNetwork.gameObject.transform, false);

				// GameObject.DontDestroyOnLoad(VKGameObject);
			}

			_isLoggedIn.Value = VkApi.VkApiInstance.IsUserLoggedIn;

			if (_isLoggedIn.Value)
				_mobileAdapter.OnLogin(this);

			vkPromise.Resolve();

			return vkPromise;
		}

		public override Promise Login()
		{
			var result = new Promise();

			login(result.Resolve, result.Reject);

			void login(Action onLoginCallback, Action onCancelCallback)
			{
				if (VkApi.VkApiInstance.IsUserLoggedIn)
				{
					onLogin();
					return;
				}

				removeListeners();

				VkApi.VkApiInstance.LoggedIn += onLogin;
				VkApi.VkApiInstance.AccessDenied += onAccessDenied;

				VkApi.VkApiInstance.Login();

				void onLogin()
				{
					removeListeners();

					// AccessToken class will have session details
					var aToken = VkApi.CurrentToken.access_token;
					// Print current access token's User ID
					Log("UserId: " + VkApi.CurrentToken.user_id);
					Log("AccessToken: " + aToken);

					_isLoggedIn.Value = VkApi.VkApiInstance.IsUserLoggedIn;

					if (_isLoggedIn.Value)
						_mobileAdapter.OnLogin(this);

					onLoginCallback?.Invoke();
				}

				void onAccessDenied(object sender, Error error)
				{
					removeListeners();
					onCancelCallback();
				}

				void removeListeners()
				{
					VkApi.VkApiInstance.LoggedIn -= onLogin;
					VkApi.VkApiInstance.AccessDenied -= onAccessDenied;
				}
			}

			return result;
		}

		public override void Logout()
		{
			if ( VkApi.VkApiInstance.IsUserLoggedIn)
				VkApi.VkApiInstance.Logout();

			_isLoggedIn.Value = VkApi.VkApiInstance.IsUserLoggedIn;

			_mobileAdapter.OnLogout(this);
		}

		public override Promise<List<SocialProfile>> GetFriends(int curPage = 0)
		{
			var result = new Promise<List<SocialProfile>>();

			var offset = (curPage - 1) * MaxCountFriendsFromNetwork;

			string method = $"apps.getFriendsList?extended=1&count={MaxCountFriendsFromNetwork}&offset={offset}&type=invite&fields={_fields}";

			var r = new VKRequest {url = method, CallBackFunction = OnGetFriendsInfo};
			VkApi.VkApiInstance.Call(r);

			return result;

			void OnGetFriendsInfo(VKRequest request)
			{
				Log("OnGetFriendsInfo => " + request.response);

				var list = OnGetUserInfo(request);

				if (list != null)
					result.Resolve(list);
				else
					result.Reject(null);
			}
		}

		public override Promise<List<SocialProfile>> GetAppFriends()
		{
			var result = new Promise<List<SocialProfile>>();

			string method = "friends.getAppUsers?";
			VKRequest r = null;

			Log("GetAppFriends + " + method);

			r = new VKRequest {url = method, CallBackFunction = afterGetUids};
			VkApi.VkApiInstance.Call(r);

			return result;

			void afterGetUids(VKRequest request)
			{
				Log("afterGetUids => " + request.response);

				var dict = Json.Deserialize(request.response) as Dictionary<string,object>;

				if (dict != null && dict.TryGetValue("response", out object val))
				{
					var users = val as List<object>;

					if (users != null)
					{
						method = $"users.get?user_ids={string.Join(",", users)}&fields={_fields}";

						r = new VKRequest {url = method, CallBackFunction = OnGetAppFriendsInfo};
						VkApi.VkApiInstance.Call(r);
					}
					else
					{
						Log("afterGetUids() no users");
						result.Reject(null);
					}
				}
				else
				{
					Log("afterGetUids() no response");

					result.Reject(null);
				}
			}

			void OnGetAppFriendsInfo(VKRequest request)
			{
				Log("OnGetAppFriendsInfo => " + request.response);

				var list = OnGetUserInfo(request);

				if (list != null)
					result.Resolve(list);
				else
					result.Reject(null);
			}
		}

		public override Promise<SocialProfile> GetProfile()
		{
			var result = new Promise<SocialProfile>();

			var method = $"users.get?user_ids={VkApi.CurrentToken.user_id}&fields={_fields}";

			var r = new VKRequest {url = method, CallBackFunction = OnGetInfo};
			VkApi.VkApiInstance.Call(r);

			return result;

			void OnGetInfo(VKRequest request)
			{
				Log("GetProfile => " + request.response);

				var list = OnGetUserInfo(request);

				if (list != null && list.Count > 0)
					result.Resolve(list[0]);
				else
					result.Reject(null);
			}
		}

		private List<SocialProfile> OnGetUserInfo(VKRequest request)
		{
			if(request.error != null)
			{
				// if(request.error.error_code == "5"){
				// 	SceneManager.LoadScene ("LoginScene");
				// }else
				// 	FindObjectOfType<GlobalErrorHandler>().Notification.Notify(request);
				//hande error here
				Log(request.error.error_msg);
				return null;
			}

			//now we need to deserialize response in json from vk server
			var dict = Json.Deserialize(request.response) as Dictionary<string,object>;

			if (dict != null && dict.TryGetValue("response", out object val))
			{
				var users = val as List<object>;

				if (users != null)
				{
					var vkUsers = VKUser.Deserialize(users.ToArray());

					return CreateProfiles(vkUsers);
				}
				else
				{
					Log("OnGetUserInfo() no users");
					return new List<SocialProfile>();
				}
			}

			Log("OnGetUserInfo() no response");

			return new List<SocialProfile>();

		}

		private List<SocialProfile> CreateProfiles(List<VKUser> data)
		{
			return data.Select(item => CreateProfile(item)).ToList();
		}

		private SocialProfile CreateProfile(VKUser data)
		{
			if (data == null) return null;

			var result = new SocialProfile();

			result.Uid = data.id.ToString();
			result.FirstName = data.first_name;
			result.LastName = data.last_name;
			result.Avatar = data.photo_100;

			return result;
		}
	}
}