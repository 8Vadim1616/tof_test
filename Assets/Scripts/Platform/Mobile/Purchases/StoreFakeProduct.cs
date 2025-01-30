namespace Assets.Scripts.Platform.Mobile.Purchases
{
    public class StoreFakeProduct : IStoreProduct
    {
        public string ProductId { get; }
        public string Price { get; }
        public string Currency => "USD";

        public bool HasTrial => false;
        public string Description => "";
        public string Title => "";

        public StoreFakeProduct(string productId, string price)
        {
            ProductId = productId;
            Price = price;
        }
    }
}
