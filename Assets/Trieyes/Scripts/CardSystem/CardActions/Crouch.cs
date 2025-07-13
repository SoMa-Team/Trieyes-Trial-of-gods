using Stats;
using System.Collections.Generic;
using CharacterSystem;
using CardSystem;
using UnityEngine;
using Utils;
using System;

namespace CardActions
{
    /// <summary>
    /// 앉기(Crouch) 카드 액션을 구현하는 클래스입니다.
    /// OnBattleSceneChange 이벤트 발생 시 캐릭터의 방어력을 증가시킵니다.
    /// </summary>
    public class Crouch : CardAction
    {
        public StatType statType1 = StatType.Defense;
        public int baseValue = 10;

        public Crouch()
        {
            actionParams = new List<ActionParam>
            {
                // 첫 번째 파라미터: 스탯타입(기본값: Defense)
                ActionParamFactory.Create(ParamKind.StatType, card => statType1),
                // 두 번째 파라미터: 증가량 (레벨에 따라)
                ActionParamFactory.Create(ParamKind.Number, card => baseValue * card.cardEnhancement.level.Value)
            };
        }

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null) return;
            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                Card card = param as Card;
                if (card == null) return;

                // 스티커 오버라이드 포함 최종 파라미터 값
                StatType statType = (StatType)GetEffectiveParam(0, card);
                int value = Convert.ToInt32(GetEffectiveParam(1, card));

                owner.statSheet[statType].AddBuff(new StatModifier(value, BuffOperationType.Additive));
                Debug.Log($"<color=yellow>[Crouch] {statType} +{value}. New Value: {owner.statSheet[statType].Value}</color>");
            }
        }
    }
}