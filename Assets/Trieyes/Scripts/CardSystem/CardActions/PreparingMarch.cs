using UnityEngine;
using CharacterSystem;
using CardSystem;
using Stats;
using System.Collections.Generic;
using BattleSystem;
using Utils;
using System;

namespace CardActions
{
    /// <summary>
    /// 전진 준비(PreparingMarch) 카드 액션을 구현하는 클래스입니다.
    /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 공격력을 증가시킵니다.
    /// </summary>
    public class PreparingMarch : CardAction
    {
        public StatType statType1 = StatType.AttackPower;
        public int baseValue = 7;

        public PreparingMarch()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card => statType1),
                ActionParamFactory.Create(ParamKind.Number, card => baseValue * card.cardEnhancement.level.Value),
            };
        }

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                Card card = param as Card;
                if (card == null) return;
                
                StatType statType = (StatType)GetEffectiveParam(0, card);
                int value = Convert.ToInt32(GetEffectiveParam(1, card));

                var modifier = new StatModifier(value, BuffOperationType.Additive);
                owner.statSheet[statType].AddBuff(modifier);

                Debug.Log($"<color=yellow>[PreparingMarch] {statType} +{value}. New Value: {owner.statSheet[statType].Value}</color>");
            }
        }
    }
}