using Stats;
using Utils;

namespace StickerSystem
{
    public class Sticker
    {
        public StickerType type;
        public int numberValue;          // type==Number일 때만 사용
        public StatType statTypeValue;   // type==StatType일 때만 사용
        public int lifeTime;
        
        public Sticker DeepCopy()
        {
            return new Sticker
            {
                type = this.type,
                numberValue = this.numberValue,
                statTypeValue = this.statTypeValue,
                lifeTime = this.lifeTime
            };
        }
    }
}