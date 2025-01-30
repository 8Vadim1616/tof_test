using System;
using System.Collections.Generic;
using Assets.Scripts.Network.Logs;
using Assets.Scripts.Network.Queries.Operations;
using UnityEngine;

namespace Assets.Scripts.Network.Queries
{
	public interface IOperation<out TRequest, out TResponse>
		where TRequest : BaseRequest
		where TResponse : BaseResponse
	{
		TRequest RequestObject { get; }
		TResponse ResponseObject { get; }
	}

	public abstract class Operation<TRequest, TResponse> : IOperation<TRequest, TResponse>
		where TRequest : BaseRequest
		where TResponse : BaseResponse
	{
		public event Action<TResponse> Success;
		public event Action<Exception> Error;

		public TRequest RequestObject { get; private set; }
		public TResponse ResponseObject { get; private set; }
		public Exception ExceptionObject { get; private set; }
		public OperationOptions Options { get; private set; }
		/// <summary>
		///  Нужно ли обрабатывать ошибку при отсутствии интернета
		/// </summary>
		public virtual bool NeedCheckInternet => false;
		/// <summary>
		/// Если true, то при ошибке будет показываться окно
		/// </summary>
		public virtual bool NeedShowWindowError => false;
		/// <summary>
		/// Если true, то при закрытии окна ошибки будет отсылаться запрос ещё раз
		/// </summary>
		public virtual bool NeedSendRequestOnErrorClose => true;
		/// <summary>
		/// Нужно ли добавлять запрос в fllogs2
		/// </summary>
		public virtual bool NeedLog => true;

		public Operation()
		{
			Options = new OperationOptions()
			{ 
				NeedCheckInternet = NeedCheckInternet, 
				NeedShowWindowError = NeedShowWindowError, 
				NeedSendRequestOnErrorClose = NeedSendRequestOnErrorClose
			};
		}

		private float _creationTime;
		public void SendCreateTimeIfNeed()
		{
			_creationTime = Time.time;
			if (Options.NeedCheckInternet && Options.NeedShowWindowError && RequestObject is BaseApiRequest baseApiRequest)
			{
				var data = new Dictionary<string, object>
						   {
							   {"action", baseApiRequest.Action},
							   {"t", _creationTime},
							   {"seq", baseApiRequest.Sequence}
						   };
				ServerLogs.SendLog("req_start_time", data);
			}
		}
		
		private void SendOnResponseTimeIfNeed()
		{
			if (Options.NeedCheckInternet && Options.NeedShowWindowError && RequestObject is BaseApiRequest baseApiRequest)
			{
				var data = new Dictionary<string, object>
						   {
							   {"action", baseApiRequest.Action},
							   {"t", Time.time - _creationTime},
							   {"seq", baseApiRequest.Sequence}
						   };
				ServerLogs.SendLog("req_end_time", data);
			}
		}

		protected void SetRequestObject(TRequest request)
		{
			RequestObject = request;
		}

		internal virtual void OnResponse(TResponse response)
		{
			SendOnResponseTimeIfNeed();
			ResponseObject = response;
			Success?.Invoke(ResponseObject);
		}

		internal virtual void OnError(Exception ex)
		{
			ExceptionObject = ex;
			Error?.Invoke(ex);
		}

		public override string ToString() => $"[{GetType().Name}]";

		public abstract string GetRequestFile();
	}

	public class OperationOptions
	{
		/// <summary>Нужно ли обрабатывать ошибку при отсутствии интернета</summary>
		public bool NeedCheckInternet;

		/// <summary>Если true, то при ошибке будет показываться окно</summary>
		public bool NeedShowWindowError;

		/// <summary>Если true, то при закрытии окна ошибки будет отсылаться запрос ещё раз</summary>
		public bool NeedSendRequestOnErrorClose = true;
	}
}