using Stats;
using UnityEngine;
using EventType = Utils.EventType;

namespace RelicSystem.RelicActions
{
    public class Relic0106_SnipersEye: RelicAction
    {
        // 치명타 확률이 10% 증가합니다.
        public override bool OnEvent(EventType eventType, object param)
        {
            switch (eventType)
            {
                case EventType.OnRelicAdded:
                    if (param is not Relic relic)
                        return false;
                    if (relic != _relic)
                        return false;
                    
                    OnRelicAdded();
                    return true;
            }
            
            return base.OnEvent(eventType, param);
        }

        private void OnRelicAdded()
        {
            var pawn = _relic.owner;
            pawn.statSheet[StatType.CriticalRate].AddToBasicValue(10);
        }
    }
}