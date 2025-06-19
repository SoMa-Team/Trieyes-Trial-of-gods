using Stats;
using System.Collections.Generic;

namespace CardActions
{
    public class CardAction002 : CardAction
    {
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // 모든 스탯을 2배로 증가
                List<StatInfo> doubledStats = new List<StatInfo>();
                foreach (var stat in owner.getAllCardStatInfos())
                {
                    doubledStats.Add(new StatInfo(stat.statType, stat.value * 2));
                }
                owner.setAllCardStatInfos(doubledStats);
                
            }
        }
    }
}