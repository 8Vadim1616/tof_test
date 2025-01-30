using System;
using Assets.Scripts.Network.Queries.ServerObjects;
using Assets.Scripts.Utils;

namespace Assets.Scripts.User
{
    public class UserEnergy
    {
        public int MaxEnergy { get; protected set; }
        public long EnergyTimeWhenFull { get; protected set; }
        public int BonusEnergy { get; protected set; }
        public long EnergyTime { get; protected set; }

		private long _energyTimeWhenEmpty => EnergyTimeWhenFull - MaxEnergy * EnergyTime;
		
        /// <summary> Текущий уровень энергии </summary>
        private int _underEnergy => Math.Min((int)((GameTime.Now - _energyTimeWhenEmpty) / EnergyTime), MaxEnergy);

        /// <summary> Текущий уровень энергии </summary>
        public int Energy => _underEnergy + BonusEnergy;

        /// <summary> Сколько времени в секундах осталось до возобновленя единицы энергии </summary>
        public int TimeLeftForEnergy => (Energy >= MaxEnergy)
            ? 0
            : (int) (EnergyTime - (EnergyTimeWhenFull - GameTime.Now) % EnergyTime);

        public long TimeLeftForFullEnergy => (_underEnergy >= MaxEnergy)
            ? 0L
            : Math.Max(0L, EnergyTimeWhenFull - GameTime.Now);

        public void Update(ServerUserInfo userData)
        {
            if (userData.EnergyTimeWhenFull.HasValue)
                EnergyTimeWhenFull = userData.EnergyTimeWhenFull.Value;

            if (userData.BonusEnergy.HasValue)
                BonusEnergy = userData.BonusEnergy.Value;

            if (userData.MaxEnergy.HasValue)
                MaxEnergy = userData.MaxEnergy.Value;

            if (userData.EnergyTime.HasValue)
                EnergyTime = userData.EnergyTime.Value;
        }
    }
}