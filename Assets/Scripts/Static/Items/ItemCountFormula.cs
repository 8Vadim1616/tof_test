using Assets.Scripts.Static.Items;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using org.mariuszgromada.math.mxparser;

namespace Assets.Scripts.Static.HR
{
	[JsonObject(MemberSerialization.OptIn)]
	public class ItemCountFormula : ItemCount
	{
		[JsonProperty("formula", DefaultValueHandling = DefaultValueHandling.Ignore)]
		private string _formula;

		public ItemCountFormula()
		{
		}
		
		public ItemCountFormula(int itemId, long count)
		{
			ItemId = itemId;
			Count = count;
		}
		
		public ItemCountFormula(Item item, long count)
		{
			ItemId = item.Id;
			Count = count;
		}

		public ItemCountFormula(int itemId, string formula)
		{
			ItemId = itemId;
			_formula = formula;
		}
		
		public ItemCountFormula(Item item, string formula)
		{
			if (item != null)
				ItemId = item.Id;
			_formula = formula;
		}

		private const string TIER = "tier";
		private const string N_TIER = "n_tier";
		private const string N = "n";

		public void ForceCreateExpression()
		{
			_valueExpression ??= CreateValueExpression();
		}
		
		private Expression _valueExpression;
		private Expression ValueExpression => _valueExpression ??= CreateValueExpression();
		private Expression CreateValueExpression()
		{
			var result = new Expression(_formula);
			result.defineArgument(TIER, 0);
			result.defineArgument(N_TIER, 0);
			result.defineArgument(N, 0);
			
			result.disableImpliedMultiplicationMode();
			_userUpgradesArguments = result.getMissingUserDefinedArguments();
			foreach (var arg in _userUpgradesArguments)
				result.defineArgument(arg, 0f);
							
			return result;
		}
		
		private string[] _userUpgradesArguments;

		public long GetCount() => GetCount(0, 0, 0);

		public long GetCount(int level, int entityLevel, int upgradeByLevel)
		{
			if (_formula.IsNullOrEmpty())
				return Count;
			
			ValueExpression.setArgumentValue(TIER, entityLevel);
			ValueExpression.setArgumentValue(N_TIER, upgradeByLevel);
			ValueExpression.setArgumentValue(N, level);
			
			if (!_userUpgradesArguments.IsNullOrEmpty())
				foreach (var arg in _userUpgradesArguments)
					ValueExpression.setArgumentValue(arg, UserFormula.GetFormulaArgumentValue(arg));
			
			var count = (long) ValueExpression.calculate();
			return count > 0 ? count : 0;
		}

		/**
		 * @param level - сквозной уровень апгрейда
		 * @param entityLevel - уровень апгрейда здания
		 * @param upgradeLevel - стадия апгрейда внутри уровня
		 */
		internal ItemCount GetItemCount(int level, int entityLevel, int upgradeByLevel)
		{
			var count = GetCount(level, entityLevel, upgradeByLevel);
			return count > 0 ? Item.CreateItemCount(count) : null;
		}
	}
}