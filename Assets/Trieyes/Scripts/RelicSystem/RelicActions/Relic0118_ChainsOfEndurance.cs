using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0118_ChainsOfEndurance: RelicAction
    {
        // 전투 시작 시 공격 속도가 50% 감소합니다. 플레이어의 공격이 적중할 때마다 공격 속도가 1% 증가합니다. 이 효과는 최대 100회 발동합니다.
        private int stackCount = 0;
        private const int maxStackCount = 100;
        
        public override bool OnEvent(EventType eventType, object param)
        {
            var owner = _relic.owner;
            
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    stackCount = 0;
                    owner.statSheet[StatType.AttackSpeed].AddBuff(new StatModifier(-50, BuffOperationType.Additive));
                    return true;
                
                case EventType.OnAttackHit:
                    if (stackCount++ > maxStackCount)
                        return false;
                    
                    owner.statSheet[StatType.AttackSpeed].AddBuff(new StatModifier(1, BuffOperationType.Additive));
                    return true;
            }
            return base.OnEvent(eventType, param);
        }
    }
}