using Stats;
using UnityEngine;
using EventType = Utils.EventType;
using Math = System.Math;

namespace RelicSystem.RelicActions
{
    public class Relic0111_Swiftheart: RelicAction
    {
        private int stackCount = 0;
        
        // 유물 획득 시마다 공격속도가 10% 증가합니다. 단 이 유물로 증가된 공격속도는 최대 200%입니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnRelicAdded:
                    stackCount = Math.Min(stackCount + 1, 20);
                    return true;
                
                case EventType.OnBattleStart:
                    if (_relic.owner == null)
                    {
                        Debug.LogError($"Relic0111_Swiftheart: 유물이 장착된 대상이 없습니다. owner is null");
                        return false;   
                    }
                    
                    var integerStatValue = _relic.owner.statSheet[StatType.AttackSpeed];
                    var modifier = new StatModifier(10 * stackCount, BuffOperationType.Multiplicative);
                    integerStatValue.AddBuff(modifier);

                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}