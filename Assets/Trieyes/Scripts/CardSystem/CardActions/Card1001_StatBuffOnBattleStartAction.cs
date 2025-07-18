using Stats;
using System.Collections.Generic;
using CharacterSystem;
using CardSystem;
using Utils;
using System;
using UnityEngine;

namespace CardActions
{
    /// <summary>
    /// 전투 시작 시 지정 스탯에 [카드 레벨 × 지정값] 만큼 버프를 부여하는 범용 액션.
    /// 자식 클래스에서 카드마다 이름/설명만 달리 사용.
    /// </summary>
    public class Card1001_StatBuffOnBattleStartAction : CardAction
    {
        private const int statTypeIndex = 0;
        private const int valueIndex = 1;

        public Card1001_StatBuffOnBattleStartAction()
        {
            actionParams = new List<ActionParam>
            {
                // 스탯타입 (예: 공격력, 방어력)
                ActionParamFactory.Create(ParamKind.StatType, card =>
                {
                    string raw = card.baseParams[0];
                    return StatTypeTransformer.KoreanToStatType(raw);
                }),
                // 증가량 (레벨 적용)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[1];
                    if (!int.TryParse(raw, out int baseValue))
                        throw new InvalidOperationException($"[{GetType().Name}] baseParams[1] 변환 실패: {raw}");
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning($"[{GetType().Name}] owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                var statType = (StatType)GetEffectiveParam(statTypeIndex);
                int value = Convert.ToInt32(GetEffectiveParam(valueIndex));
                var modifier = new StatModifier(value, BuffOperationType.Additive);
                owner.statSheet[statType].AddBuff(modifier);

                Debug.Log($"<color=yellow>[{GetType().Name}] {statType} +{value} (New Value: {owner.statSheet[statType].Value})</color>");
            }
        }
    }
}
