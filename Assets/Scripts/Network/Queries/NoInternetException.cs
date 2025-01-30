using System;

namespace Assets.Scripts.Network.Queries
{
	public class NoInternetException : Exception
	{
		public NoInternetException() : base("InternetReachability NotReachable") { }
	}
}