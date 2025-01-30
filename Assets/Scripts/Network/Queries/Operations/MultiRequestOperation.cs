using System.Collections.Generic;
using Assets.Scripts.Libraries.RSG;
using Newtonsoft.Json;

namespace Assets.Scripts.Network.Queries.Operations
{
    public class MultiRequestOperation : BaseApiOperation<BaseApiMultiRequest, BaseApiResponse>
    {
        public MultiRequestOperation(List<BaseRequest> requests)
        {
            SetRequestObject(new BaseApiMultiRequest() { requests = requests }); 
        }
    }

    public class BaseApiMultiRequest : BaseApiRequest
    {
        [JsonProperty("requests")]
        public List<BaseRequest> requests = new List<BaseRequest>();

        public BaseApiMultiRequest() : base("requests")
        {

        }

        public override void PrepareToSend()
        {
            requests.Each(r => r.SetAsMulti());
            base.PrepareToSend();
        }
    }
}
