using Stats;
using Utils;

namespace StickerSystem
{
    public class Sticker
    {
        public string instanceId { get; private set; } = System.Guid.NewGuid().ToString("N");
        
        public StickerType type;
        public int numberValue;
        public StatType statTypeValue;
        public int lifeTime;
        
        public Sticker DeepCopy()
        {
            var clone = (Sticker)MemberwiseClone();
            return clone;
        }
    }
}