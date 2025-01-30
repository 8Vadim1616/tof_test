#if UNITY_EDITOR || !UNITY_WEBGL && !UNITY_WSA && !UNITY_STANDALONE
using HuaweiMobileServices.IAP;

namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class HuaweiProduct : IStoreProduct
    {
        private readonly ProductInfo _mobileProduct;
        public string Price => _mobileProduct.Price;

        public bool HasTrial => _mobileProduct.PriceType == PriceType.IN_APP_SUBSCRIPTION && !string.IsNullOrEmpty(_mobileProduct.SubPeriod);

        public string Currency => _mobileProduct?.Currency ?? "USD";
        public string Description => _mobileProduct?.ProductDesc ?? "";
        public string Title => _mobileProduct.SubGroupTitle ?? "";

        public HuaweiProduct(ProductInfo mobileProduct)
        {
            _mobileProduct = mobileProduct;
        }
    }
}
#endif
