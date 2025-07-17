using Stats;

namespace StickerSystem
{
    public enum StickerType
    {
        StatType,
        Number,
    }

    public class Sticker
    {
        public StickerType type;
        public int numberValue;          // type==Number일 때만 사용
        public StatType statTypeValue;   // type==StatType일 때만 사용
        public int lifeTime;
    }

}