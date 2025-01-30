using UnityEngine;

namespace Assets.Scripts.Animations
{
    public class AnimationData
    {
        public string Name { get; }
        public int Hash { get; }

        public AnimationData(string name)
        {
            Name = name;
            Hash = Animator.StringToHash(name);
        }

        public static bool operator == (AnimationData a1, AnimationData a2)
        {
            if (a1 is null)
                return a2 is null;

            return a1.Equals(a2);
        }

        public static bool operator != (AnimationData a1, AnimationData a2) =>
            !(a1 == a2);

        public override bool Equals(object obj) =>
            obj is AnimationData a2 && Hash == a2.Hash;

        public override int GetHashCode() =>
            Hash;
    }
}