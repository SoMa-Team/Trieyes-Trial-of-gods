using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0114_WingedBoots: RelicAction
    {
        // 전투가 시작할 때, 이동속도를 10% 증가시킵니다.    

        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    
                    var integerStatValue = _relic.owner.statSheet[StatType.MoveSpeed];
                    var modifier = new StatModifier(10, BuffOperationType.Multiplicative);
                    integerStatValue.AddBuff(modifier);
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}