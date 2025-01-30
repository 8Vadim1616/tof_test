namespace Assets.Scripts.Network.Queries.Operations
{
    public class OnlineOperation : BaseApiOperation<OnlineOperation.Request, BaseApiResponse>
    {
        public OnlineOperation()
        {
            SetRequestObject(new Request() {});
        }

        public class Request : BaseApiRequest
        {
            public Request() : base("online")
            {
            }
        }
    }
}