namespace Assets.Scripts.Network.Queries
{
    public class BaseRequest : IRequest
    {
        public virtual void PrepareToSend() { }
        public virtual void SetAsMulti() { }
    }
}
