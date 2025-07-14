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
        public Crouch()
        {
            actionParams = new List<ActionParam>
            {
                // 첫 번째 파라미터: 스탯타입 (CSV에서 예: Defense)
                ActionParamFactory.Create(ParamKind.StatType, card =>
                {
                    string raw = card.baseParams[0];
                    return StatTypeTransformer.ParseStatType(raw);
                }),
                // 두 번째 파라미터: 증가량 (CSV에서 예: 10)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[1];
                    if (!int.TryParse(raw, out int baseValue))
                        throw new InvalidOperationException($"[Crouch] baseParams[1] 변환 실패: {raw}");
                    return baseValue * card.cardEnhancement.level.Value;
                }),
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
                StatType statType = (StatType)GetEffectiveParam(0);
                int value = Convert.ToInt32(GetEffectiveParam(1));

                owner.statSheet[statType].AddBuff(new StatModifier(value, BuffOperationType.Additive));
                Debug.Log($"<color=yellow>[Crouch] {statType} +{value}. New Value: {owner.statSheet[statType].Value}</color>");
            }
        }
    }
}