using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Network.Queries.ServerObjects.Sockets;

namespace Assets.Scripts.User.Sockets
{
	public class UserSocketChannels
	{
		public UserData User { get; private set; }
		public List<ServerSocketChannel> All = new List<ServerSocketChannel>();

		public UserSocketChannels(UserData user)
		{
			User = user;
		}

		public ServerSocketChannel FriendInfoChannel => GetByName("ch:" + User.Uid + ":frndinfo");

		public ServerSocketChannel GetByName(string name)
		{
			return All.FirstOrDefault(channel => channel.Name.Equals(name));
		}

		public bool Update(Dictionary<string, ServerSocketChannel> data)
		{
			if (data == null) return false;

			All = data.Values.ToList();
			return true;
		}
	}
}