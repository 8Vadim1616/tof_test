using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations;
using Assets.Scripts.UI.Windows;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using IServiceProvider = Assets.Scripts.GameServiceProvider.IServiceProvider;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Network.Queries
{
	public class QueryManager : IServiceProvider
	{
		private const string TAG = "[QueryManager] ";
		public static string SALT
		{
			get
			{
				if (BuildSettings.BuildSettings.IsRelease)
					return "MAanas9aWz651qnh";

				return "MAanas9aWz651qnh";
			}
		}

		public const string REQUEST_LOCK_KEY = "REQUEST_LOCK_KEY";
		public const string QUERY_MANAGER_WIN_LOCK = "QueryManager";

		public string Server { get; set; }
		public int DefaultRequestTimeout = 15;
		public int Attempts = 3;
		public int BadInternetTimeout = 5;

		public bool ShowUidInGet = true;

		private readonly Queue<RequestBundle> requestsQueue = new Queue<RequestBundle>();
		private bool inProcess;
		private uint sequence = 1;
		private uint lastGotSequence = 0;
		private bool canAddNewRequestToQueue = true;
		private bool isWaitingForLastAnswer = false;

		public const string GAME_LOGIC_KEY = "game_logic_error";
		public const string SECOND_COPY_KEY = "second_game_copy";
		public const string SERVER_NOT_AVAILABLE = "server_not_available";
		public const string ERROR_SEQUENCE = "Err seq";

		public bool IsValid { get; private set; } = true;
		public Promise OnConnect { get; set; }

		private Promise waitForLastAnswerPromise;

		public QueryManager()
		{
			OnConnect = Promise.Resolved() as Promise;
		}

		/// <summary>
		/// Делает QueryManager нерабочим. Не может посылать запросы пока не произойдет ResetManager
		/// </summary>
		public void Invalidate()
		{
			IsValid = false;
		}

		public void Init(bool relogin)
		{
		}
		
		public void UnlockNewRequestsQueue()
		{
			canAddNewRequestToQueue = true;
		}

		public void LockNewRequestsQueue()
		{
			canAddNewRequestToQueue = false;
		}

		public bool IsLastSequenceGot
		{
			get => lastGotSequence == sequence - 1;
		}

		public IPromise WaitForLastAnswers()
		{
			isWaitingForLastAnswer = true;
			waitForLastAnswerPromise = new Promise();
			/*if (!isMultiRequestProcessing)
				waitForLastAnswerPromise.ResolveOnce();*/
			if (IsLastSequenceGot)
				waitForLastAnswerPromise.ResolveOnce();

			return waitForLastAnswerPromise;
		}

		public void ResetManager()
		{
			//очистка очереди
			if (requestsQueue.Count > 0)
			{
				var requestBundle = requestsQueue.Peek();
				requestsQueue.Clear();
				requestsQueue.Enqueue(requestBundle);
			}
			sequence = 1;
			lastGotSequence = 0;
			IsValid = true;
			canAddNewRequestToQueue = true;
			isWaitingForLastAnswer = false;
			waitForLastAnswerPromise?.ResolveOnce();
			waitForLastAnswerPromise = null;
		}

		private uint GetSequence() { Debug.Log(TAG + "--> Got sequence [" + sequence + "]"); return sequence++; }

		private readonly List<IOperation<BaseRequest, BaseResponse>> queueMulti = new List<IOperation<BaseRequest, BaseResponse>>();

		public void MultiRequest<TRequest, TResponse>(Operation<TRequest, TResponse> operation)
						where TRequest : BaseRequest
						where TResponse : BaseResponse, new()
		{
			queueMulti.Add(operation);
			CheckMultiQuery();
		}

		private bool isMultiRequestProcessing;
		private void CheckMultiQuery(bool force = false)
		{
			if (isMultiRequestProcessing && !force) return;
			if (!canAddNewRequestToQueue) return;
			if (queueMulti.Count == 0) return;
			if (isWaitingForLastAnswer) return;

			List<BaseRequest> requests = new List<BaseRequest>();
			foreach (var multi in queueMulti)
				if (multi.RequestObject != null)
					requests.Add(multi.RequestObject);

			queueMulti.Clear();
			isMultiRequestProcessing = true;

			RequestPromise(new MultiRequestOperation(requests), false).Then(
																			response =>
																			{
																				if (response.Responses != null)
																				{
																					foreach (var resp in response.Responses)
																					{
																						if (resp != null)
																							Game.ServerDataUpdater.Update(resp);
																					}
																				}

																				isMultiRequestProcessing = false;
																				if (!isWaitingForLastAnswer)
																					/*waitForLastAnswerPromise?.ResolveOnce();
																				else */
																					CheckMultiQuery();
																			}
																		   );
		}

		public void ForceSendMultiRequest()
		{
			CheckMultiQuery(true);
		}


		public IPromise<TResponse> RequestPromise<TRequest, TResponse>(Operation<TRequest, TResponse> pOperation, bool needLock = true)
						where TRequest : BaseRequest
						where TResponse : BaseResponse, new()
		{
			if (!canAddNewRequestToQueue)
				return Promise<TResponse>.Rejected(new Exception("Can't send"));

			CheckMultiQuery(true);

			if (needLock && Game.Locker != null)
			{
				Game.Locker.Lock(REQUEST_LOCK_KEY);
				if (Game.Loader != null && Game.Settings?.NEED_SHOW_LOADER_WHEN_QUERY_LOCKS_SCREEN == true)
					Game.Loader.Show();
			}

			var result = new Promise<TResponse>();

			pOperation.Success += onSuccess;
			pOperation.Error += onError;

			Request(pOperation);

			return result;

			void onSuccess(TResponse response)
			{
				if (needLock && Game.Locker != null)
				{
					if (Game.Loader != null)
						Game.Loader.Hide();
					Game.Locker.Unlock(REQUEST_LOCK_KEY);
				}

				if (response.ProfileChanged.HasValue && response.ProfileChanged == true && response.RemoteProfile != null)
				{
					//todo
					// if (Game.TutorController?.IsAnyTutorActive == false && Game.Instance.PlayField == null)
					// {
					// 	Game.Windows.AddNewWindowsCreationAvailableLockOnce(QUERY_MANAGER_WIN_LOCK);
					// 	//Game.Windows.NewWindowsCreationAvailable = false;
					// 	var remoteProfile = response.RemoteProfile;
					//
					// 	Game.Instance.GameLoadingPromise
					// 		.Then(() => ChooseSaveWindow.Of(remoteProfile, onLoadRemoteProfile, onChooseLocalProfile));
					//
					// 	return;
					// }
				}

				if (isWaitingForLastAnswer && IsLastSequenceGot)
					waitForLastAnswerPromise?.ResolveOnce();

				result.Resolve(response);
			}

			void onError(Exception ex)
			{
				if (isWaitingForLastAnswer && IsLastSequenceGot)
					waitForLastAnswerPromise?.ResolveOnce();

				result.Reject(ex);
			}

			void onLoadRemoteProfile()
			{
				result.Reject(null);
				GameReloader.Reload(true, true);
			}

			void onChooseLocalProfile()
			{
				result.Reject(null);

				pOperation.Success -= onSuccess;
				pOperation.Error -= onError;

				RequestPromise(pOperation)
					.Then(_ => GameReloader.Reload(true, true));
			}
		}

		private void Request<TRequest, TResponse>(Operation<TRequest, TResponse> operation) where TRequest : BaseRequest where TResponse : BaseResponse, new()
		{
			if (string.IsNullOrEmpty(Server))
				throw new Exception(TAG + "Invalid parameters");
			if (operation == null)
				throw new Exception(TAG + "Operation is null");

			var requestObject = operation.RequestObject;
			if (requestObject == null)
				throw new Exception(TAG + "Hasn't RequestObject");

			requestObject.PrepareToSend();

			uint currentSequence = 0;
			string action = "";
			
			Uri uri;
			
			if (requestObject is BaseApiRequest apiRequest)
			{
				var parameters = new NameValueCollection
				{
					{"action", apiRequest.Action},
					{"auth_key", apiRequest.AuthKey}
				};
				uri = GetRequest(operation.GetRequestFile(), parameters);
				
				action = apiRequest.Action;
				currentSequence = GetSequence();
				apiRequest.Sequence = currentSequence;
				operation.SendCreateTimeIfNeed();
				GameLogger.debug(TAG + "--> Operation ready " + operation);
			}
			else
			{
				uri = GetRequest(operation.GetRequestFile());
			}

			if (action.Equals("player.tower"))
			{
				GameLogger.debug(TAG + "--> Operation ready " + operation);
				
			}
			
			var jsonRequestString = JsonConvert.SerializeObject(requestObject);
			var signature = Utils.Utils.Hash(jsonRequestString + SALT);

			GameLogger.debug(TAG + "--> Request " + uri + " " + operation + "\r\n" + jsonRequestString);

			SendRequest(operation.Options, uri, signature, jsonRequestString, action)
				.Then(request =>
				{
					var jsonStringAnswer = request.downloadHandler.text;
					GameLogger.debug(TAG + "<-- Response " + operation + "\r\n" +
									(jsonStringAnswer.Length < 8096
													? jsonStringAnswer
													: "[Too Long Response String]"));

					if (lastGotSequence < currentSequence)
						lastGotSequence = currentSequence;

					TResponse response = new TResponse();
							   
					if (response is INotJsonResponse notJsonResponse)
					{
						notJsonResponse.ResponseText = jsonStringAnswer;
						response.OnParsed(jsonStringAnswer);
					}
					else
					{
						try
						{
							response = JsonConvert.DeserializeObject<TResponse>(jsonStringAnswer);
							response.OnParsed(jsonStringAnswer);
						}
						catch (Exception e)
						{
							var dict = new Dictionary<string, string> {{"error", e.Message}};
							var obj = JsonConvert.SerializeObject(dict);
							response = JsonConvert.DeserializeObject<TResponse>(obj);
							GameLogger.error(e);
						}
					}

					Game.ExecuteOnMainThread(() =>
					{
						if (!string.IsNullOrEmpty(response.Warning))
							GameLogger.warning($"{TAG} <-- Response Warning {response.Warning} {operation}");
						if (!string.IsNullOrEmpty(response.Error))
						{
							//GameLogger.error($"{TAG} <-- Response Error {response.Error} operation");
							if (response.Error.Contains(ERROR_SEQUENCE))
								OnError(request, QueryErrorType.SecondCopy, response.Error);
							else
								OnError(request, QueryErrorType.GameLogic, response.Error);

							operation.OnError(new ServerLogicException(response.Error));
							return;
						}

						operation.OnResponse(response);
					});
				})
				.Catch(ex =>
				{
					operation.OnError(ex);
				});
		}

		private class RequestBundle
		{
			public int Attempt { get; set; }
			public Promise<UnityWebRequest> Callback { get; private set; }
			public OperationOptions OperationOptions { get; private set; }

			public Uri Uri { get; private set; }
			public string Signature { get; private set; }
			public string JsonRequestString { get; private set; }
			public string Action { get; }

			public RequestBundle(OperationOptions operationOptions, Promise<UnityWebRequest> callback, Uri uri, string signature, string jsonRequestString, string action)
			{
				Attempt = 0;
				Callback = callback;
				OperationOptions = operationOptions;

				Uri = uri;
				Signature = signature;
				JsonRequestString = jsonRequestString;
				Action = action;
			}

			public string GetRnd() =>
				 Uri.UnescapeDataString(HttpUtility.ParseQueryString(Uri.Query).Get("rnd"));
			
			public UnityWebRequest CreateRequest()
			{
				return new UnityWebRequest
				{
					url = Uri.ToString(),
					method = UnityWebRequest.kHttpVerbPOST,
					downloadHandler = new DownloadHandlerBuffer(),
					uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(Signature + JsonRequestString)) { contentType = "application/json" },
					disposeUploadHandlerOnDispose = true,
					disposeDownloadHandlerOnDispose = true,
				};
			}
		}

		private IPromise<UnityWebRequest> SendRequest(OperationOptions operationOptions, Uri uri, string signature, string jsonRequestString, string action)
		{
			var promise = new Promise<UnityWebRequest>();
			requestsQueue.Enqueue(new RequestBundle(operationOptions, promise, uri, signature, jsonRequestString, action));
			Process();

			return promise;
		}

		public ReactiveProperty<bool> IsGoodInternet { get; } = new ReactiveProperty<bool>(true);

		private void Process()
		{
			if (!IsValid) return;
			if (inProcess) return;
			if (requestsQueue.Count == 0) return;

			var requestBundle = requestsQueue.Peek();
			var promise = requestBundle.Callback;
			UnityWebRequest request = null;

			if (!Game.Network.NetworkReachable)
			{
				if (onConnectionError())
					return;
			}

			request = requestBundle.CreateRequest();
			request.timeout = DefaultRequestTimeout;

			inProcess = true;

			GameLogger.debug(TAG + "--> SendWebRequest " + request.url);

			IPromise badInternetTimerPromise = null;
			badInternetTimerPromise = Utils.Utils.Wait(BadInternetTimeout)
										   .Then(() =>
										   {
											   if (badInternetTimerPromise != null)
												   IsGoodInternet.Value = false;
										   });

			request.SendWebRequest().completed += operation =>
			{
				Debug.Log(TAG + "SendRequest Completed. Result=" + request.result + "; code=" + request.responseCode);
				inProcess = false;

				badInternetTimerPromise = null;
				IsGoodInternet.Value = true;

				if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
				{
					requestsQueue.Dequeue();

					promise.Resolve(request);
					Process();
					request.Dispose();
					return;
				}
				else
				{
					var exception = new Exception(request.result.ToString() + " code=" + request.responseCode);

					if (request.result == UnityWebRequest.Result.ConnectionError ||
						request.result == UnityWebRequest.Result.ProtocolError ||
						request.result == UnityWebRequest.Result.DataProcessingError)
					{
#if UNITY_EDITOR
						Debug.LogError(TAG + "SendRequest Error. Result=" + request.result + "; code=" + request.responseCode);
#endif	
						
						exception = new NoInternetException();

						if (onConnectionError())
						{
							request.Dispose();
							return;
						}
					}

					requestsQueue.Dequeue();
					promise.Reject(exception);
					Process();
					request.Dispose();
					return;
				}
			};

			bool onConnectionError()
			{
				if (IsQueueContainsNeedCheckInternet())
				{
					GameLogger.warning(TAG + "InternetReachability NotReachable");
					OnError(request, QueryErrorType.Network, TAG + "InternetReachability NotReachable", false);

					//if (requestBundle.OperationOptions.NeedShowWindowError && requestBundle.OperationOptions.NeedSendRequestOnErrorClose)
					if (IsQueueContainsNeedShowWindowErrorAndNeedSendRequestOnErrorClose()) //В окне будет попытка перепослать запрос, промис резолвить нельзя
					{

					}
					else
					{
						foreach (var req in requestsQueue)
							req.Callback.Reject(new NoInternetException());
						requestsQueue.Clear();
						//requestsQueue.Dequeue();
						//promise.Reject(new NoInternetException());
					}

					return true;
				}

				return false;
			}
		}
		
		private bool IsQueueContainsNeedShowWindowErrorAndNeedSendRequestOnErrorClose()
		{
			return requestsQueue?.FirstOrDefault(r => r.OperationOptions.NeedShowWindowError && r.OperationOptions.NeedSendRequestOnErrorClose) != null;
		}
		
		private bool IsQueueContainsNeedCheckInternet()
		{
			return requestsQueue?.FirstOrDefault(r => r.OperationOptions.NeedCheckInternet) != null;
		}

		private bool IsQueueContainsNeedShowWindowError()
		{
			return requestsQueue?.FirstOrDefault(r => r.OperationOptions.NeedShowWindowError) != null;
		}
		
		private bool IsQueueContainsNeedSendRequestOnErrorClose()
		{
			return requestsQueue?.FirstOrDefault(r => r.OperationOptions.NeedSendRequestOnErrorClose) != null;
		}

		private Uri GetRequest(string requestFile, NameValueCollection parameters = null)
		{
			if (parameters == null)
				parameters = new NameValueCollection();
			parameters["rnd"] = Random.Range(1, int.MaxValue).ToString();

			if (ShowUidInGet && Game.User != null && !string.IsNullOrEmpty(Game.User.Uid))
				parameters["uid"] = Game.User.Uid;

			if (parameters.Count == 0)
				return new Uri(Server + requestFile);

			var parametersString = string.Join("&", parameters.AllKeys.Select(key => string.Format("{0}={1}", key, Utils.Utils.Escape(parameters[key]))).ToArray());

			return new Uri(Server + requestFile + "?" + parametersString);
		}

		private void OnError(UnityWebRequest unityWebRequest, QueryErrorType errorType, string message = null, bool needLogError = true) 
		{
			if (Game.Locker)
				Game.Locker.Unlock(REQUEST_LOCK_KEY);

			if (Game.Loader)
				Game.Loader.Hide();

			long responseCode = -1;
			if (unityWebRequest != null)
				responseCode = unityWebRequest.responseCode;
			
			if (needLogError)
			{
				if (errorType == QueryErrorType.GameLogic)
				{
					GameLogger.error(message != null
													 ? $"{TAG}Query Error [{errorType}]: {message}"
													 : $"{TAG}Query Error [{errorType}]");
				}
				else
				{
					GameLogger.warning(message != null
													   ? $"{TAG}Query Error [{errorType}]: {message}"
													   : $"{TAG}Query Error [{errorType}]");
				}
			}

			if (!IsQueueContainsNeedShowWindowError())
				return;

			Game.Windows.HolderSetTopOrder();

			var key = GAME_LOGIC_KEY;

			switch (errorType)
			{
				case QueryErrorType.Network:
				case QueryErrorType.Server:
					key = SERVER_NOT_AVAILABLE;
					break;

				case QueryErrorType.SecondCopy:
					key = SECOND_COPY_KEY;
					break;
			}

			IEnumerable<string> _wasLocks;
			showWindow(key, message);

			void showWindow(string key = null, string message = null)
			{
				_wasLocks = null;
				if (Game.Locker)
				{
					_wasLocks = Game.Locker.GetAllLocks();
					Game.Locker.ClearAllLocks();
				}

				var data = new Dictionary<string, object>
			    {
					{"actions", string.Join(",", requestsQueue.Select(r => r.Action))},
					{"rnds", string.Join(",", requestsQueue.Select(r => r.GetRnd()))},
					{"responseCode", responseCode}
			    };

				if (errorType == QueryErrorType.Network)
				{
					InfoWindow.Of("no connection", "No connection");
				}
				else
					InfoWindow.OfError(key, message, onClick, data);
			}

			void onClick()
			{
				if (Game.Locker)
					Game.Locker.Lock(_wasLocks);
				
				Game.Windows.RemoveAllNewWindowsCreationAvailableLocks(QUERY_MANAGER_WIN_LOCK);
				//Game.Windows.NewWindowsCreationAvailable = true;
				Game.Windows.HolderSetBaseOrder();
				if (IsQueueContainsNeedSendRequestOnErrorClose())
					Process();
			}
		}

		#region Logs

		public static byte[] Zip(string str)
		{
			var bytes = Encoding.UTF8.GetBytes(str);

			using var msi = new MemoryStream(bytes);
			using var mso = new MemoryStream();
			using (var gs = new GZipStream(mso, CompressionMode.Compress))
			{
				CopyTo(msi, gs);
			}

			return mso.ToArray();
		}

		public static void CopyTo(Stream src, Stream dest)
		{
			byte[] bytes = new byte[4096];

			int cnt;

			while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
			{
				dest.Write(bytes, 0, cnt);
			}
		}

		public enum LogType
		{
			COMMON,
			CRASH
		}

		public void SendLog(LogType type, string logText, string name = null)
		{
			if (logText.IsNullOrEmpty())
				return;

			var parameters = new NameValueCollection
							 {
											 {"uid", Game.User.Uid},
											 {"sn", Game.Social.Network},
											 {"ver", Application.version},
							 };

			if (!string.IsNullOrEmpty(name))
				parameters.Add("name", name);

			Uri uri = GetRequest(type == LogType.CRASH ? "crash_log.php" : "gamelog.php", parameters);

			var request = new UnityWebRequest
			{
				url = uri.ToString(),
				method = UnityWebRequest.kHttpVerbPOST,
				downloadHandler = new DownloadHandlerBuffer(),
				uploadHandler = new UploadHandlerRaw(Zip(logText))
				{
					contentType = "application/octet-stream"
				},
				disposeUploadHandlerOnDispose = true,
				disposeDownloadHandlerOnDispose = true,
			};

			var nameData = name != null ? " " + name + " " : " ";
			GameLogger.debug(TAG + "Send Log" + nameData + "-> " + uri);

			request.SendWebRequest().completed += operation =>
			{
				GameLogger.debug(TAG + "SendLog Completed with: " + (string.IsNullOrEmpty(request.error) ? "OK" : "Error " + request.error));
				request.Dispose();
			};
		}

		#endregion

	}
}