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
    /// '전진 준비' 카드의 효과 구현.
    /// 전투 시작 시 지정 스탯(공격력 등)을 [카드 레벨 × 지정값] 만큼 증가시킨다.
    /// </summary>
    public class PreparingMarch : CardAction
    {
        private const int firstStatTypeIndex = 0;
        private const int firstValueIndex = 1;
        public PreparingMarch()
        {
            // actionParams[0]: 증가시킬 스탯 타입 (ex. 공격력)
            // actionParams[1]: 증가값 (카드 레벨 적용)
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                {
                    // baseParams[0]: 한글(공격력 등) → StatType으로 변환
                    string raw = card.baseParams[0];
                    return StatTypeTransformer.KoreanToStatType(raw);
                }),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    // baseParams[1]: 증가 수치 (정수) × 카드 레벨
                    string raw = card.baseParams[1];
                    if (!int.TryParse(raw, out int baseValue))
                        throw new InvalidOperationException($"[PreparingMarch] baseParams[1] 변환 실패: {raw}");
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
                Debug.LogWarning("[PreparingMarch] owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // (1) 실제 적용 스탯/수치 가져오기 (스티커/레벨 영향 반영)
                var statType = (StatType)GetEffectiveParam(firstStatTypeIndex); // 0: 스탯 종류
                int value = Convert.ToInt32(GetEffectiveParam(firstValueIndex)); // 1: 수치

                // (2) 버프 생성 및 적용
                var modifier = new StatModifier(value, BuffOperationType.Additive);
                owner.statSheet[statType].AddBuff(modifier);

                Debug.Log($"<color=yellow>[PreparingMarch] {statType} +{value} (New Value: {owner.statSheet[statType].Value})</color>");
            }
        }
    }
}
