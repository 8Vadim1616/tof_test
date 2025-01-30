using System;
using Assets.Scripts.Libraries.RSG;

namespace Assets.Scripts.Platform.Adapter.Actions
{
	public class SocialActionHandler
	{
		public Type Action { get; private set; }

		public Func<ISocialAction, Promise<object>> Handler { get; private set; }

		public SocialActionHandler(Type action, Func<ISocialAction, Promise<object>> handler)
		{
			Action = action;
			Handler = handler;
		}
	}
}