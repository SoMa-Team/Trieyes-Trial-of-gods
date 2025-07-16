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
    /// '웅크리기(Crouch)' 카드의 효과 구현.
    /// 전투 시작 시 지정 스탯(예: 방어력)을 [카드 레벨 × 지정값] 만큼 증가시킨다.
    /// </summary>
    public class Crouch : CardAction
    {
        public Crouch()
        {
            actionParams = new List<ActionParam>
            {
                // actionParams[0]: 적용 스탯타입 (CSV 예: Defense)
                ActionParamFactory.Create(ParamKind.StatType, card =>
                {
                    string raw = card.baseParams[0];
                    return StatTypeTransformer.KoreanToStatType(raw);
                }),
                // actionParams[1]: 증가량 (CSV 예: 10)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[1];
                    if (!int.TryParse(raw, out int baseValue))
                        throw new InvalidOperationException($"[Crouch] baseParams[1] 변환 실패: {raw}");
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }

        /// <summary>
        /// 전투 시작(씬 전환) 시 효과 발동
        /// </summary>
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[Crouch] owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // (1) 실제 적용 스탯/수치 가져오기 (스티커/레벨 영향 반영)
                var statType = (StatType)GetEffectiveParam(0); // 0: 스탯 종류
                int value = Convert.ToInt32(GetEffectiveParam(1)); // 1: 수치

                // (2) 버프 생성 및 적용
                var modifier = new StatModifier(value, BuffOperationType.Additive);
                owner.statSheet[statType].AddBuff(modifier);

                Debug.Log($"<color=yellow>[Crouch] {statType} +{value} (New Value: {owner.statSheet[statType].Value})</color>");
            }
        }
    }
}
