using Stats;
using Utils;

namespace StickerSystem
{
    public class Sticker
    {
        public string instanceId { get; private set; } = System.Guid.NewGuid().ToString("N");
        
        public StickerType type;
        public int numberValue;          // type==Number일 때만 사용
        public StatType statTypeValue;   // type==StatType일 때만 사용
        public int lifeTime;
        
        public Sticker DeepCopy()
        {
            var clone = (Sticker)MemberwiseClone();
            return clone;
        }
    }
}