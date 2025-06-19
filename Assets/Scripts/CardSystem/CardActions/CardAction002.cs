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
                // 모든 스탯을 2배로 증가
                foreach (StatType statType in System.Enum.GetValues(typeof(StatType)))
                {
                    int currentValue = owner.statSheet[statType].Value;
                    owner.statSheet[statType].SetBasicValue(currentValue * 2);
                }
            }
        }
    }
}