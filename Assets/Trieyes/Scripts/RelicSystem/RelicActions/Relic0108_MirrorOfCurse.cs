using AttackSystem;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0108_MirrorOfCurse: RelicAction
    {
        // 플레이어가 입은 데미지 만큼의 데미지를 적에게 입힙니다. 단, 엘리트, 보스에는 적용되지 않습니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnDamageHit:
                    if (param is not AttackResult attackResult)
                        return false;
                    var attacker = attackResult.attacker;
                    // TODO : Enemy Type Filter
                    if ("Need To Add Condition (Enemy is Elite or Boss)" is null)
                        return false;
                    attacker.ChangeHP(-attackResult.totalDamage);
                    return true;
            }
            return base.OnEvent(eventType, param);
        }
    }
}