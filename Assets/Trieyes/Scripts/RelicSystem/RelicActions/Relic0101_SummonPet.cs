using AttackSystem;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0101_SummonPet: RelicAction
    {
        private int attackID = 1;

        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    // AttackFactory.Instance.Create();
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }
    }
}