using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;

namespace CardActions
{
    public class Card0402_WeightOfArmor : CardAction
    {
        private const int upStatTypeIdx = 0;
        private const int upValueCoefIdx = 1;
        private const int downStatTypeIdx = 2;
        private const int downValueCoefIdx = 3;

        public Card0402_WeightOfArmor()
        {
            actionParams = new List<ActionParam>
            {
                // [0] 방어력 스탯 타입
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                // [1] 올라가는 수치 계수 (예: 10)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[upValueCoefIdx];
                    int.TryParse(raw, out int baseValue);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
                // [2] 이동속도 스탯 타입
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[downStatTypeIdx])),
                // [3] 내려가는 수치 계수 (예: 1)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[downValueCoefIdx];
                    int.TryParse(raw, out int baseValue);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[ArmorWeightParam] owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // "갑옷"이 이름에 포함된 카드 개수
                int armorCardCount = 0;
                foreach (var card in deck.Cards)
                {
                    if (card == null || string.IsNullOrEmpty(card.cardName)) continue;
                    if (card.cardName.Contains("갑옷"))
                        armorCardCount++;
                }

                if (armorCardCount == 0)
                {
                    Debug.Log("[ArmorWeightParam] '갑옷'이 포함된 카드 없음.");
                    return;
                }

                var upStat = (StatType)GetEffectiveParam(upStatTypeIdx);
                int upValuePercent = Convert.ToInt32(GetEffectiveParam(upValueCoefIdx)); // ex: 20
                var downStat = (StatType)GetEffectiveParam(downStatTypeIdx);
                int downValuePercent = Convert.ToInt32(GetEffectiveParam(downValueCoefIdx)); // ex: 2

                owner.statSheet[upStat].AddBuff(new StatModifier(upValuePercent, BuffOperationType.Multiplicative));
                owner.statSheet[downStat].AddBuff(new StatModifier(-1*downValuePercent, BuffOperationType.Multiplicative));

                Debug.Log($"<color=cyan>[ArmorWeightParam] '{owner.gameObject.name}' - '갑옷' 카드 {armorCardCount}장: {upStat} ×{upValuePercent:F3}, {downStat} ×{downValuePercent:F3} 적용</color>");
            }
        }
    }
}