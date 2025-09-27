using AttackSystem;
using RelicSystem;
using UnityEngine;
using EventType = Utils.EventType;

public class Relic0121_MultiShot : RelicAction
{
    private const int attackID = 3004;
    private const float duration = 1f;
    
    private float lastTriggered;
    
    // 플레이어 피격 시, 플레이어 공격력의 25% 데미지를 주는 유도 공격을 12개 발사합니다.
    public override bool OnEvent(EventType eventType, object param)
    {
        switch (eventType)
        {
            case EventType.OnBattleStart:
                lastTriggered = Time.time - duration;
                return false;
            
            case EventType.OnDamaged:
                if (Time.time - lastTriggered < duration)
                    return false;
                
                AttackFactory.Instance.CreateByID(attackID, _relic.owner, null, Vector2.zero);
                lastTriggered = Time.time;
                return true;
        }
        return base.OnEvent(eventType, param);
    }
}
