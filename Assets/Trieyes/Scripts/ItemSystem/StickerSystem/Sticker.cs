using Stats;

namespace StickerSystem
{
    public enum StickerType
    {
        Number,
        StatType
    }

    public class Sticker
    {
        public StickerType type;
        public int numberValue;          // type==Number일 때만 사용
        public StatType statTypeValue;   // type==StatType일 때만 사용
        public int lifeTime;

        // 생성자: StickerType/범위 등 인자로 직접 만듦
        public Sticker(int value, int life) // 숫자 스티커
        {
            type = StickerType.Number;
            numberValue = value;
            lifeTime = life;
        }
        public Sticker(StatType statType, int life) // 스탯 스티커
        {
            type = StickerType.StatType;
            statTypeValue = statType;
            lifeTime = life;
        }
    }

}