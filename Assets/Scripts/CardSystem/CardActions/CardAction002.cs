using Stats;
using System.Collections.Generic;
using UnityEngine;

namespace CardActions
{
    [System.Serializable]
    public class CardAction002 : CardAction
    {
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                if (owner != null)
                {
                    // Defense * 2
                    var modifier = new StatModifier(100, BuffOperationType.Multiplicative);
                    owner.statSheet[StatType.Defense].AddBuff(modifier);
                    Debug.Log($"<color=yellow>[CardAction002] Applied: DEF * 2. New Value: {owner.statSheet[StatType.Defense].Value}</color>");
                }
            }
        }
    }
}