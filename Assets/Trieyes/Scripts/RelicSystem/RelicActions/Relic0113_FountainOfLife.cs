using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0113_FountainOfLife: RelicAction
    {
        // 매 전투 종료 시, HP가 전체 HP의 10%만큼 회복합니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleEnd:
                    var owner = _relic.owner;
                    var changedHP = owner.maxHp / 10;
                    owner.ChangeHP(changedHP);
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}