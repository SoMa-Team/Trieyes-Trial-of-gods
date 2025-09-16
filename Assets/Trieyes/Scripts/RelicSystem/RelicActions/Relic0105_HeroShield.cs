using System.Linq;
using Stats;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0105_HeroShield: RelicAction
    {
        // Relic 105
        // 방어력이 10% 증가합니다. [용사의 검] 유물과 같이 장착될 경우, 방어력이 100% 더 증가합니다.
        private int pairRelicId = 104;

        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnBattleStart:
                    return OnBattleStart();
            }

            return base.OnEvent(eventType, param);
        }

        private bool OnBattleStart()
        {
            if (_relic.owner == null)
                return false;

            var owner = _relic.owner;
            var condition = owner.relics.Select(relic => relic.relicID).Any(id => id == pairRelicId);
            var value = condition ? 110 : 10;

            owner.statSheet[StatType.Defense]
                .AddBuff(new StatModifier(value, BuffOperationType.Multiplicative, false, -1));
            return true;
        }
    }
}