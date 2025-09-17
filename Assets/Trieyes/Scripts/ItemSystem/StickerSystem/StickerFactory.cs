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
        public static Sticker CreateNumberSticker(int value, int lifeTime = 1)
        {
            var sticker = new Sticker();
            ActivateNumberSticker(sticker, value, lifeTime);
            return sticker;
        }

        public static Sticker CreateStatTypeSticker(StatType statType, int lifeTime = 1)
        {
            var sticker = new Sticker();
            ActivateStatTypeSticker(sticker, statType, lifeTime);
            return sticker;
        }

        public static Sticker CreateProbabilitySticker(int probability, int lifeTime = 1)
        {   
            var sticker = new Sticker();
            ActivateProbabilitySticker(sticker, probability, lifeTime);
            return sticker;
        }

        private static void ActivateNumberSticker(Sticker sticker, int value, int lifeTime)
        {
            sticker.type = StickerType.Number;
            sticker.numberValue = value;
            sticker.lifeTime = lifeTime;
        }

        private static void ActivateProbabilitySticker(Sticker sticker, int value, int lifeTime)
        {
            sticker.type = StickerType.Probability;
            sticker.numberValue = value;
            sticker.lifeTime = lifeTime;
        }

        private static void ActivateStatTypeSticker(Sticker sticker, StatType statType, int lifeTime)
        {
            sticker.type = StickerType.StatType;
            sticker.statTypeValue = statType;
            sticker.lifeTime = lifeTime;
        }

        public static Sticker CreateRandomSticker(int minVal = 1, int maxVal = 101, int minLifeTime = 1, int maxLifeTime = 10, int minProb = 1, int maxProb = 40)
        {
            var types = System.Enum.GetValues(typeof(StickerType));
            var type = (StickerType)types.GetValue(Random.Range(1, types.Length));
            switch (type)
            {
                case StickerType.Number:
                    return CreateNumberSticker(Random.Range(minVal, maxVal), Random.Range(minLifeTime, maxLifeTime));
                case StickerType.Probability:
                    return CreateProbabilitySticker(Random.Range(minProb, maxProb), Random.Range(minLifeTime, maxLifeTime));
                case StickerType.StatType:
                    var stat = StickerStats[Random.Range(0, StickerStats.Length)];
                    return CreateStatTypeSticker((StatType)stat, Random.Range(minLifeTime, maxLifeTime));
                default:
                    Debug.LogError("Unknown sticker type");
                    return null;
            }
        }
    }
}
