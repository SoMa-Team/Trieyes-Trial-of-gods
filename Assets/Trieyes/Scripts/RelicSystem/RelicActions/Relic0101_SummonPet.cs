using AttackSystem;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0101_SummonPet: RelicAction
    {
        // Relic_101
        // 전투를 도와주는 위습을 소환합니다. 1초에 플레이어 공격력의 10% 데미지를 주는 유도 공격을 발사합니다
        private int attackID = 1;

        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    // TODO : AttackFactory.Instance.Create();
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}