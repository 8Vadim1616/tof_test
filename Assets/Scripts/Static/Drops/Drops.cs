using System.Collections.Generic;

namespace Assets.Scripts.Static.Drops
{
    public class Drops
    {
        private readonly Dictionary<int, Drop> _allDropsById;

        public Drops(Dictionary<int, Drop> data) =>
            _allDropsById = data ?? new Dictionary<int, Drop>();

        public Drop this[int id] =>
            GetDrop(id);

        public Drop GetDrop(int id) =>
            _allDropsById.ContainsKey(id) ? _allDropsById[id] : null;
    }
}
