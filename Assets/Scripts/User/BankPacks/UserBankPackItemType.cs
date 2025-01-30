namespace Assets.Scripts.User.BankPacks
{
    public class UserBankPackItemType
    {
        public const int TYPE_NONE = 0;
        public const int TYPE_PACK = 1;
        public const int TYPE_PACK_REAL = 2;
        public const int TYPE_OFFER = 3;
        public const int TYPE_ADVERT = 4;

		public static int GetTypeByString(string str)
        {
            switch (str)
            {
                case "pack":
                    return TYPE_PACK;
                case "pack_real":
                    return TYPE_PACK_REAL;
                case "offer":
                    return TYPE_OFFER;
                case "advert":
                    return TYPE_ADVERT;
			}

            return TYPE_NONE;
        }
    }
}