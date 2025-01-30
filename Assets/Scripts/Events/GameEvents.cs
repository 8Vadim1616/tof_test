namespace Assets.Scripts.Events
{
    public static class GameEvents
    {
        public class UserDataUpdateEvent : Event { }

        public class UserAdsUpdated : Event { }
		
        public class UserAdsWatchStatusUpdate : Event
        {
            public enum AdUpdateStatus
            {
                None, Closed, Successed, Failed
            }
            
            public AdUpdateStatus Status { get; private set; }

            public UserAdsWatchStatusUpdate() { Status = AdUpdateStatus.None; }

            public UserAdsWatchStatusUpdate(AdUpdateStatus status) { Status = status; }
        }

        public class SomeProductInintedEvent : Event
        {
            public string ProductId { get; private set; }

            public SomeProductInintedEvent(string productId)
            {
                ProductId = productId;
            }
        }

        // public class SomeOfferAddedEvent : Event
        // {
        //     public OfferIcon OfferIcon { get; private set; }
        //
        //     public SomeOfferAddedEvent(OfferIcon offerIcon)
        //     {
        //         OfferIcon = offerIcon;
        //     }
        // }

        public class DeleteDataOnGameReload : Event { }
		public class UserCommunityMembershipUpdated : Event { }
	}
}
