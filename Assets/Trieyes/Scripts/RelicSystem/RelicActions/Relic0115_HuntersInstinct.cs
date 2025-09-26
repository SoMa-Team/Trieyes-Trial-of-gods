using Stats;
using UnityEngine;
using EventType = Utils.EventType;

namespace RelicSystem.RelicActions
{
    public class Relic0115_HuntersInstinct: RelicAction
    {
        // 적을 처치할 때마다, 3초 동안 이동속도가 20% 증가하고, 공격속도가 20% 증가합니다. 이 효과는 중첩되지 않습니다.
        private float availableTime;
        
        private const float duration = 3;
        
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    availableTime = Time.time;
                    break;
                
                case EventType.OnKilled:
                    if (Time.time < availableTime)
                        return false;
                    
                    var moveSpeed = _relic.owner.statSheet[StatType.MoveSpeed];
                    var modifier = new StatModifier(20, BuffOperationType.Additive, false, duration);
                    moveSpeed.AddBuff(modifier);
                    
                    var attackSpeed = _relic.owner.statSheet[StatType.MoveSpeed];
                    var modifier2 = new StatModifier(20, BuffOperationType.Additive, false, duration);
                    attackSpeed.AddBuff(modifier2);
                    
                    availableTime = Time.time + duration;
                    
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}