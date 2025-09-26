using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0119_CrystalOrb: RelicAction
    {
        // 전투 시작 후, 20초간 마력이 30% 증가합니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            
            if (eventType == EventType.OnBattleStart)
            {
                var owner = _relic.owner;
                var modifier = new StatModifier(30, BuffOperationType.Multiplicative, false, 20);
                owner.statSheet[StatType.MagicPower].AddBuff(modifier);
                return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}