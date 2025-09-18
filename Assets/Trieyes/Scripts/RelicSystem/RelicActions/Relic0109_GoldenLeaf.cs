using BattleSystem;
using PrimeTween;
using Utils;

namespace RelicSystem.RelicActions
{
    public class Relic0109_GoldenLeaf : RelicAction
    {
        // 적 처치시 10% 확률로 추가 1골드가 드랍됩니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnKilled:
                    Gold gold = DropFactory.Instance.CreateGold(_relic.owner.transform.localPosition, 1, false);
                    DropFactory.Instance.AnimationDrop(gold);
                    BattleStage.now.AttachGold(gold);
                    return true;
            }

            return base.OnEvent(eventType, param);
        }
    }
}