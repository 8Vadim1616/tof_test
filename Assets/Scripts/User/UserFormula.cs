using System;
using System.Collections.Generic;
using Assets.Scripts.Static.HR;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.User
{
	public class UserFormula
	{
		private const string PLAYER_LEVEL = "level";
		private const string GPS_PREFIX = "gps_";
		private const string ITEM_COUNT = "item__";

		private static readonly Dictionary<string, Func<long>> ArgumentFuncs = new Dictionary<string, Func<long>>();

		private static Func<long> GetArgumentFunc(string argumentName)
		{
			if (argumentName.StartsWith(GPS_PREFIX))
				return Game.Static.Items.Get(argumentName).ItemCountFormula.GetCount;

			if (argumentName.StartsWith(ITEM_COUNT))
			{
				var item = Game.Static.Items.Get(argumentName.Substring(ITEM_COUNT.Length));
				return () => Game.User.Items.GetCount(item);
			}

			var _ = argumentName.IndexOf("__");
			var hero = argumentName.Substring(0, _);
			var characteristic = argumentName.Substring(_ + 2);

			// if (Enum.TryParse(hero, true, out UpgradeHero heroEn) && Enum.TryParse(characteristic, true, out UpgradeType upgradeType))
			// {
			// 	return () => Game.User.Upgrades.GetCharacteristicValue(heroEn, upgradeType).Value;
			// }

			Debug.LogError($"Cant parse formula argument {argumentName}");
			return () => 0;
		}

		public static long GetFormulaArgumentValue(string argumentName)
		{
			if (!ArgumentFuncs.ContainsKey(argumentName))
				ArgumentFuncs[argumentName] = GetArgumentFunc(argumentName);

			return ArgumentFuncs[argumentName]();
		}
	}
}