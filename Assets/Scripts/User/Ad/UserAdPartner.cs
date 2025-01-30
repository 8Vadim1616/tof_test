using System.Collections.Generic;

namespace Assets.Scripts.User.Ad
{
	public class UserAdPartner
	{
		private static Dictionary<int, object> _mobileAdvertPartners = new Dictionary<int, object>();

		/** Серверная рекламная точка (5, 105, 205 ...) */
		public int ServerPoint { get; private set; }

		/** Рекламный партнер (admob, admob2, pm8, ...) */
		public string AdvertName { get; private set; }

		public bool IsPartnerSDKAvailable => AdvertName.Contains("admob");

		/** Доступна ли реклама с сервера */
		public bool ServAvailable { get; set; }

		public UserAdPartner(string name, int serverPoint)
		{
			AdvertName = name;
			ServerPoint = serverPoint;
		}

		/*public static function pushPartner(name : String, partnerType : String) : void
		{
			var cl : Class = MobileAdvertPartnerType.getClassByType(partnerType);
			var partner : AbstractMobileAdvertPartner = new cl(name);
			_mobileAdvertPartners[name] = partner;
		}

		public static function initPartners() : void
		{
			for(var name : String in _mobileAdvertPartners)
			{
				//Инициализируем недостающих партнеров (если они пришли в процессе игры после инициализации рекламы
				if(!getPartner(name).isInited())
				{
					logI(name + " init");
					getPartner(name).init();
				}

				getPartner(name).update();
			}
		}

		public static function getPartner(name : String) : AbstractMobileAdvertPartner
		{
			return _mobileAdvertPartners[name];
		}

		public static function forEach(callback : Function) : void
		{
			for each (var mobileAdvertPartner : AbstractMobileAdvertPartner in _mobileAdvertPartners)
			{
				callback(mobileAdvertPartner);
			}
		}*/





		// public function get mobileAdvertPartner() : AbstractMobileAdvertPartner
		// {
		// 	return getPartner(_advertName);
		// }

		public bool Available =>
			ServAvailable && IsPartnerSDKAvailable; /*&& mobileAdvertPartner && mobileAdvertPartner.hasReward()*/

	}
}