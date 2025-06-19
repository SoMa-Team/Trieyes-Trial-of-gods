using Stats;
using System.Collections.Generic;

namespace CardActions
{
    public class CardAction002 : CardAction
    {
        public CardAction002() : base(2, "전투 시작 시 모든 스탯을 2배로 증가")
        {
        }

        public override void OnEvent(Utils.EventType eventType, object param)
        {
            if (eventType == Utils.EventType.OnBattleSceneChange && owner != null)
            {
                // Call by Reference 방식: 기존 리스트의 요소들을 직접 수정
                foreach (var stat in owner.statInfos)
                {
                    stat.Value *= 2; // 기존 StatInfo 객체의 Value를 직접 수정
                }
            }
        }
    }
}