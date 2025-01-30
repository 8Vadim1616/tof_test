using System.Linq;
using Assets.Scripts.Static.Drops;
using Assets.Scripts.Static.HR;
using Assets.Scripts.User;
using Assets.Scripts.Utils;
using Newtonsoft.Json;
using org.mariuszgromada.math.mxparser;
using UniRx;

namespace Assets.Scripts.Static.Tower
{

	public class TowerItem : StaticCollectionItemCode
	{
		public const string MONEY1 = "Money1";
		public const string EXP = "Exp";
		
		/**
		[JsonProperty("id")]
		public int Id { get; private set; }
		*/

		/**
		[JsonProperty("#id")]
		public string SharpId { get; private set; }
		*/
		
		/**
		public TowerItem()
		{
		}

		public TowerItem(float value)
		{
		}
		*/
		
		public bool IsMoney1 => this == Game.Static.TowerItems.Money1;

		public bool IsExp => this == Game.Static.TowerItems.Exp;
		
	}
	
}
