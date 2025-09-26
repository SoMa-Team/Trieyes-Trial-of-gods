using AttackSystem;
using UnityEngine;
using EventType = Utils.EventType;

namespace RelicSystem.RelicActions
{
    public class Relic0120_Pinball: RelicAction
    { 
        // 전투 시작 후, 스크린 모서리에 튕기는 공격을 생성합니다. 해당 공격은 플레이어의 공격력의 5% 데미지를 입힙니다.
        private const int attackID = 3001;

        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    AttackFactory.Instance.CreateByID(attackID, _relic.owner, null, Vector2.zero);
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}