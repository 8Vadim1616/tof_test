using Assets.Scripts.Static.Items;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Assets.Scripts.Network.Queries.Operations.Api
{
	public class AdShowOperation : BaseApiOperation<AdShowOperation.Request, AdShowOperation.Response>
	{
		public AdShowOperation(int adPoint, int adPointChild, string partnerName = null, ItemCount[] rwrds = null, bool needInfo = true, Dictionary<string, object> adParams = null)
		{
			SetRequestObject(new Request(adPoint, adPointChild, partnerName, rwrds, needInfo, adParams));
		}

		public class Request : BaseApiRequest
		{
			public readonly int adPoint;
			public readonly int adPointChild; //дочерняя точка, партнёр
			public readonly string partner;

			[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
			public readonly ItemCount[] reward;

			public readonly bool needInfo;

			public Request(int point, int childPoint, string partnerName, ItemCount[] rewards, bool needInfoReward, Dictionary<string, object> adParams) : base("adshow")
			{
				partner = partnerName;
				adPoint = point;
				adPointChild = childPoint;
				reward = rewards;
				needInfo = needInfoReward;
				AdvertParams = adParams;
			}
		}

		public class Response : BaseApiResponse
		{
		}
	}
}