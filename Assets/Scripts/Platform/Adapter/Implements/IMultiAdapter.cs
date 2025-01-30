namespace Assets.Scripts.Platform.Adapter.Implements
{
	public interface IMultiAdapter
	{
		void OnLogin(AbstractSocialAdapter adapter);
		void OnLogout(AbstractSocialAdapter adapter);
	}
}