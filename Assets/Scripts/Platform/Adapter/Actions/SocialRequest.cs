namespace Assets.Scripts.Platform.Adapter.Actions
{
	public class SocialRequest : ISocialAction
	{
		public string Uid { get; set; }
		public string Message { get; private set; }
		public string RequestKey { get; private set; }
		public string Url { get; private set; }
		public string Title { get; private set; }

		public SocialRequest(string message, string uid = "", string requestKey = "", string url = "", string title = "")
		{
			Uid = uid;
			Message = message;
			RequestKey = requestKey;
			Url = url;
			Title = title;
		}
	}
}