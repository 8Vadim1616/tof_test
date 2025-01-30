using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries.Operations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using IServiceProvider = Assets.Scripts.GameServiceProvider.IServiceProvider;
using Random = UnityEngine.Random;

namespace Assets.Scripts.Network.Queries
{
	public class QueryManagerAsync : IServiceProvider
	{
		private const string TAG = "[QueryManagerAsync] ";
		public static string SALT => QueryManager.SALT;

		public string Server { get; set; }
		public int DefaultRequestTimeout = 15;
		public bool IsValid { get; private set; } = true;
		public Promise OnConnect { get; set; }

		public QueryManagerAsync()
		{
			OnConnect = Promise.Resolved() as Promise;
		}
		
		public void MultiRequest<TRequest, TResponse>(Operation<TRequest, TResponse> operation)
						where TRequest : BaseRequest
						where TResponse : BaseResponse, new()
		{
			
		}

		public IPromise<TResponse> RequestPromise<TRequest, TResponse>(Operation<TRequest, TResponse> pOperation, bool needLock = true)
						where TRequest : BaseRequest
						where TResponse : BaseResponse, new()
		{
			var result = new Promise<TResponse>();

			pOperation.Success += onSuccess;
			pOperation.Error += onError;

			Request(pOperation);

			return result;

			void onSuccess(TResponse response) => result.Resolve(response);
			void onError(Exception ex) => result.Reject(ex);
		}
		
		/// <summary>
		/// Делает QueryManager нерабочим. Не может посылать запросы пока не произойдет ResetManager
		/// </summary>
		public void Invalidate()
		{
			IsValid = false;
		}

		public void SendLog(QueryManager.LogType type, string logText, string name = null) { }
		public void Init(bool relogin) { }

		private void Request<TRequest, TResponse>(Operation<TRequest, TResponse> operation) where TRequest : BaseRequest where TResponse : BaseResponse, new()
		{
			if (string.IsNullOrEmpty(Server))
				throw new Exception(TAG + "Invalid parameters");
			if (operation == null)
				throw new Exception(TAG + "Operation is null");

			var uri = GetRequest(operation.GetRequestFile());
			var requestObject = operation.RequestObject;
			if (requestObject == null)
				throw new Exception(TAG + "Hasn't RequestObject");

			requestObject.PrepareToSend();

			if (requestObject is BaseApiRequest apiRequest)
				GameLogger.debug(TAG + "--> Operation ready " + operation);

			var jsonRequestString = JsonConvert.SerializeObject(requestObject);
			var signature = Utils.Utils.Hash(jsonRequestString + SALT);

			GameLogger.debug(TAG + "--> Request " + uri + " " + operation + "\r\n" + jsonRequestString);

			SendRequest(operation.Options, requestCreator)
				.Then(request =>
				{
					var jsonStringAnswer = request.downloadHandler.text;
					GameLogger.debug(TAG + "<-- Response " + operation + "\r\n" +
									(jsonStringAnswer.Length < 8096
													? jsonStringAnswer
													: "[Too Long Response String]"));

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

			UnityWebRequest requestCreator()
			{
				return new UnityWebRequest
				{
					url = uri.ToString(),
					method = UnityWebRequest.kHttpVerbPOST,
					downloadHandler = new DownloadHandlerBuffer(),
					uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(signature + jsonRequestString)) { contentType = "application/json" },
					disposeUploadHandlerOnDispose = true,
					disposeDownloadHandlerOnDispose = true,
				};
			}
		}

		private class RequestBundle
		{
			public Func<UnityWebRequest> RequestCreator { get; private set; }
			public int Attempt { get; set; }
			public Promise<UnityWebRequest> Callback { get; private set; }
			public OperationOptions OperationOptions { get; private set; }

			public RequestBundle(OperationOptions operationOptions, Func<UnityWebRequest> requestCreator, Promise<UnityWebRequest> callback)
			{
				Attempt = 0;
				RequestCreator = requestCreator;
				Callback = callback;
				OperationOptions = operationOptions;
			}
		}

		private IPromise<UnityWebRequest> SendRequest(OperationOptions operationOptions, Func<UnityWebRequest> requestCreator)
		{
			var promise = new Promise<UnityWebRequest>();
			var requestBundle = new RequestBundle(operationOptions, requestCreator, promise);
			Process();
			
			void Process()
			{
				if (!IsValid) return;

				var promise = requestBundle.Callback;

				if (!Game.Network.NetworkReachable)
					return;

				var request = requestBundle.RequestCreator();
				request.timeout = DefaultRequestTimeout;

				GameLogger.debug(TAG + "--> SendWebRequest " + request.url);

				request.SendWebRequest().completed += operation =>
				{
					Debug.Log(TAG + "SendRequest Completed. Result=" + request.result + "; code=" + request.responseCode);

					if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
						promise.Resolve(request);
					else
						promise.Reject(null);
					
					request.Dispose();
				};
			}

			return promise;
		}

		private Uri GetRequest(string requestFile, NameValueCollection parameters = null)
		{
			if (parameters == null)
				parameters = new NameValueCollection();
			parameters["rnd"] = Random.Range(1, int.MaxValue).ToString();

			if (Game.User != null && !string.IsNullOrEmpty(Game.User.Uid))
				parameters["uid"] = Game.User.Uid;

			if (parameters.Count == 0)
				return new Uri(Server + requestFile);

			var parametersString = string.Join("&", parameters.AllKeys.Select(key => string.Format("{0}={1}", key, Utils.Utils.Escape(parameters[key]))).ToArray());

			return new Uri(Server + requestFile + "?" + parametersString);
		}
	}
}