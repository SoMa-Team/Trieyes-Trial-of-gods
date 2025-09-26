using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0116_ChaliceOfReversal: RelicAction
    {
        // 이 유물을 획득하는 순간, 공격력과 마력의 기본 값이 뒤바뀝니다.

        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnRelicAdded:
                    if (param is not Relic relic)
                        return false;

                    if (relic != _relic)
                        return false;
                    
                    var owner = _relic.owner;
                    var attack = owner.statSheet[StatType.AttackPower].GetBasicValue();
                    var magic = owner.statSheet[StatType.MagicPower].GetBasicValue();
                    
                    owner.statSheet[StatType.AttackPower].SetBasicValue(magic);
                    owner.statSheet[StatType.MagicPower].SetBasicValue(attack);
                    
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}