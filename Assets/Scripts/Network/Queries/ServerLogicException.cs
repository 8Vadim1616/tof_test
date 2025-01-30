using System;

namespace Assets.Scripts.Network.Queries
{
	public class ServerLogicException : Exception
	{
		public ServerLogicException(string message) : base(message) { }
	}
}