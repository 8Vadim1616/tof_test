using Assets.Scripts.Libraries.RSG;
using Assets.Scripts.Network.Queries;

namespace Assets.Scripts.GameServiceProvider
{
    public interface IServiceProvider
    {
        bool IsValid { get; }
		string Server { get; set; }
		Promise OnConnect { get; set; }

		void MultiRequest<TRequest, TResponse>(Operation<TRequest, TResponse> operation)
            where TRequest : BaseRequest
            where TResponse : BaseResponse, new();

        IPromise<TResponse> RequestPromise<TRequest, TResponse>(Operation<TRequest, TResponse> pOperation,
            bool needLock = true)
            where TRequest : BaseRequest
            where TResponse : BaseResponse, new();

        void SendLog(QueryManager.LogType type, string logText, string name = null);

		void Init(bool relogin);
	}
}