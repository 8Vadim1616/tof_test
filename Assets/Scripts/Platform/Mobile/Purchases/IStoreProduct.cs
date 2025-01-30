namespace Assets.Scripts.Platform.Mobile.Purchases
{
    
    public interface IStoreProduct
    {
        string Price { get; }
        bool HasTrial { get; }
        string Currency { get; }
        string Description { get; }
        string Title { get; }
    }
}
