using UnityEngine;
using Stats;
using Utils;

namespace StickerSystem
{
    public class StickerFactory : MonoBehaviour
    {
        public static StickerFactory Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }
        
        public Sticker CreateNumberSticker(int value, int lifeTime = 1)
        {
            var sticker = new Sticker();
            ActivateNumberSticker(sticker, value, lifeTime);
            return sticker;
        }

        public Sticker CreateStatTypeSticker(StatType statType, int lifeTime = 1)
        {
            var sticker = new Sticker();
            ActivateStatTypeSticker(sticker, statType, lifeTime);
            return sticker;
        }

        private void ActivateNumberSticker(Sticker sticker, int value, int lifeTime)
        {
            sticker.type = StickerType.Number;
            sticker.numberValue = value;
            sticker.lifeTime = lifeTime;
        }

        private void ActivateStatTypeSticker(Sticker sticker, StatType statType, int lifeTime)
        {
            sticker.type = StickerType.StatType;
            sticker.statTypeValue = statType;
            sticker.lifeTime = lifeTime;
        }

        public Sticker CreateRandomSticker(int minVal = 1, int maxVal = 101, int minLifeTime = 1, int maxLifeTime = 10)
        {
            var types = System.Enum.GetValues(typeof(StickerType));
            var type = (StickerType)types.GetValue(Random.Range(0, types.Length));
            switch (type)
            {
                case StickerType.Number:
                    return CreateNumberSticker(Random.Range(minVal, maxVal), Random.Range(minLifeTime, maxLifeTime));
                case StickerType.StatType:
                    var statValues = System.Enum.GetValues(typeof(StatType));
                    var stat = (StatType)statValues.GetValue(Random.Range(0, statValues.Length));
                    return CreateStatTypeSticker((StatType)stat, Random.Range(minLifeTime, maxLifeTime));
                default:
                    Debug.LogError("Unknown sticker type");
                    return null;
            }
        }
    }
}