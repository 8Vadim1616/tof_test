using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Static.UserGroups
{
	public class StaticUserGroups : Dictionary<int, StaticUserGroupsData>
	{
		public const string FILE_NAME = "user_group";

		public bool TryGetNextGroup(out int nextGroup)
		{
			nextGroup = 1;
			return false;
		}
	}
}