using AttackSystem;
using UnityEngine;
using EventType = Utils.EventType;

namespace RelicSystem.RelicActions
{
    public class Relic0110_JudgesSword: RelicAction
    {
        // 플레이어가 공격을 맞출경우, 1% 확률로 적 처치 효과가 발생됩니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnAttack:
                    if (!(Random.Range(0, 100) < 1))
                    {
                        var pawn = _relic.owner;
                        var attackResult = param as AttackResult;
                        pawn.OnEvent(EventType.OnKilled, attackResult);
                    }
                    break;
            }
            return base.OnEvent(eventType, param);
        }
    }
}