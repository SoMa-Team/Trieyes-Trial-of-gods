using UnityEngine;
using Stats;
using Utils;

namespace StickerSystem
{
    public static class StickerFactory
    {
        private static readonly StatType[] StickerStats = new[]
        {
            StatType.AttackPower,
            StatType.MagicPower,
            StatType.Health,
            StatType.CriticalDamage,
            StatType.Defense,
            StatType.AttackSpeed,
            StatType.SkillCooldownReduction,
        };
        public static Sticker CreateAddSticker(int value)
        {
            var s = new Sticker();
            s.type = StickerType.Add;
            s.numberValue = value;
            return s;
        }

        public static Sticker CreatePercentSticker(int percent)
        {
            var s = new Sticker();
            s.type = StickerType.Percent;
            s.numberValue = percent; // 100 => +100%
            return s;
        }

        public static Sticker CreateStatTypeSticker(StatType statType)
        {
            var s = new Sticker();
            s.type = StickerType.StatType;
            s.statTypeValue = statType;
            return s;
        }

        public static Sticker CreateRandomSticker(
            int minVal = 1, int maxVal = 100,
            int minPercent = 1, int maxPercent = 100)
        {
            var types = System.Enum.GetValues(typeof(StickerType));
            var type = (StickerType)types.GetValue(UnityEngine.Random.Range(1, types.Length));
            switch (type)
            {
                case StickerType.Add:
                    return CreateAddSticker(UnityEngine.Random.Range(minVal, maxVal));
                case StickerType.Percent:
                    return CreatePercentSticker(UnityEngine.Random.Range(minPercent, maxPercent));
                case StickerType.StatType:
                    var stat = StickerStats[UnityEngine.Random.Range(0, StickerStats.Length)];
                    return CreateStatTypeSticker(stat);
                default:
                    UnityEngine.Debug.LogError("Unknown sticker type");
                    return null;
            }
        }
    }
}
