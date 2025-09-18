using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    // 전투 시작 후, 20초간 공격력이 30% 증가합니다.
    public class Relic0107_FlagOfBattleField: RelicAction
    {
        public override bool OnEvent(EventType eventType, object param)
        {
            if (eventType == EventType.OnBattleStart)
            {
                var owner = _relic.owner;
                var modifier = new StatModifier(30, BuffOperationType.Multiplicative, false, 20);
                owner.statSheet[StatType.AttackPower].AddBuff(modifier);
                return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}