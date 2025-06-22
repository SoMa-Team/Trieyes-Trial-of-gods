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
                    // 현재 DEF 값 확인
                    int currentDef = owner.statSheet[StatType.Defense].Value;
                    Debug.Log($"<color=yellow>[CardAction002] Current DEF before buff: {currentDef}</color>");
                    
                    // Defense * 2 (100% 증가 = 2배)
                    var modifier = new StatModifier(100, BuffOperationType.Multiplicative);
                    owner.statSheet[StatType.Defense].AddBuff(modifier);
                    
                    // 버프 적용 후 DEF 값 확인
                    int newDef = owner.statSheet[StatType.Defense].Value;
                    Debug.Log($"<color=yellow>[CardAction002] Applied: DEF * 2. New Value: {newDef}</color>");
                }
            }
        }
    }
}