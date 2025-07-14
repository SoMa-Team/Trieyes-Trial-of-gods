using TagSystem;
using RelicID = System.Int32;

namespace RelicSystem
{
    public class RandomOptionGenerator
    {
        public static RandomOption Create(RelicID relicID)
        {
            // TODO: Random 생성 필요
            var randomOption = new RandomOption();
            
            randomOption.FilterTag = AttackTag.Range;
            randomOption.RelicStatType = RelicStatType.AOE;
            randomOption.value = 10;
            
            return randomOption;
        }
    }
}