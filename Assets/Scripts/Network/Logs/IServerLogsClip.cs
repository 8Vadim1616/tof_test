namespace Assets.Scripts.Network.Logs
{
	public interface IServerLogsClip
	{
		ServerLogsParams LogParams { get; }
		string ClassName { get; }
	}
}