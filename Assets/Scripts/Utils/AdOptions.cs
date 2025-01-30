using System.Collections.Generic;

namespace Assets.Scripts.Utils
{
	public class AdOptions
	{
		public int Adp_id { get; private set; }
		public int PartnerAdp_id { get; private set; }
		public int Stm { get; private set; }
		public int Tp { get; private set; } = 1;
		public Dictionary<string, object> AdvertParams { get; private set; }

		public static AdOptions Of(int advertId, int partnerPointId, Dictionary<string, object> advertParams)
		{
			return new AdOptions()
			{
				Adp_id = advertId,
				PartnerAdp_id = partnerPointId,
				Stm = (int) Game.AdvertisingController.StartAdvTime,
				Tp = 1,
				AdvertParams = advertParams
			};
		}
	}
}