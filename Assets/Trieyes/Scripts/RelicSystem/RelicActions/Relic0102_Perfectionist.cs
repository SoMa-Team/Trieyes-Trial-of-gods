using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0102_Perfectionist: RelicAction
    {
        // Relic 102    
        // 체력이 100%일 경우, 공격력이 지속적으로 증가합니다. 체력이 100%미만일 경우, 증가한 공격력이 초기화됩니다. 이 효과는 전투 종료시 초기화됩니다.
        private int stackCount = 0;
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    stackCount = 0;
                    break;
                
                case EventType.OnBattleEnd:
                    stackCount = 0;
                    break;
                
                case EventType.OnTick:
                    var owner = _relic.owner;

                    if (owner.currentHp == owner.maxHp)
                    {
                        var statModifier = new StatModifier(1, BuffOperationType.Additive, false);
                        owner.statSheet[StatType.AttackPower].AddBuff(statModifier);
                        stackCount++;
                    }

                    else if (stackCount > 0)
                    {
                        var statModifier = new StatModifier(-stackCount, BuffOperationType.Additive, false);
                        owner.statSheet[StatType.AttackPower].AddBuff(statModifier);
                        stackCount = 0;
                    }
                    
                    break;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}