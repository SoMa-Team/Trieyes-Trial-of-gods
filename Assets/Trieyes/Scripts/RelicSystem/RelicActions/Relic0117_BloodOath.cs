using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0117_BloodOath: RelicAction
    {
        // 플레이어가 피해를 입을 때마다, 공격력과 마력이 1 증가합니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnDamaged:
                    var owner = _relic.owner;
                    owner.statSheet[StatType.AttackPower].AddBuff(new StatModifier(1, BuffOperationType.Additive));
                    owner.statSheet[StatType.MagicPower].AddBuff(new StatModifier(1, BuffOperationType.Additive));
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}