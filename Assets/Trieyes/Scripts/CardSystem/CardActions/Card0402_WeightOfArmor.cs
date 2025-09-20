using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;

namespace CardActions
{
    /// <summary>
    /// 전투 시작 시, 이름에 '갑옷'이 들어간 카드 1장당
    /// - upStat: +{upValue}% (기본: 배율, 스티커로 가산/배율 전환 가능)
    /// - downStat: -{downValue}% (기본: 배율, 스티커로 가산/배율 전환 가능)
    /// </summary>
    public class Card0402_WeightOfArmor : CardAction
    {
        private const int upStatTypeIdx     = 0;
        private const int upValueCoefIdx    = 1;
        private const int downStatTypeIdx   = 2;
        private const int downValueCoefIdx  = 3;

        public Card0402_WeightOfArmor()
        {
            actionParams = new List<ActionParam>
            {
                // 증가 대상 스탯
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),

                // 증가 값(기본은 Percent). 스티커(Add/Percent)로 연산 타입 전환 가능
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValueCoefIdx]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),

                // 감소 대상 스탯
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[downStatTypeIdx])),

                // 감소 값(기본은 Percent). 스티커(Add/Percent)로 연산 타입 전환 가능
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[downValueCoefIdx]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (eventType != Utils.EventType.OnBattleSceneChange) return false;
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[WeightOfArmor] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            // "갑옷"이 이름에 포함된 카드 개수
            int armorCardCount = deck.SubstringCount("갑옷");
            if (armorCardCount == 0)
            {
                Debug.Log("[WeightOfArmor] '갑옷'이 포함된 카드 없음.");
                return false;
            }

            // === 증가 효과 ===
            var upStat = (StatType)GetEffectiveParam(upStatTypeIdx);
            var (upValPerCard, upOp) = GetBuffFromParamPreferSticker(upValueCoefIdx);
            int upTotal = upValPerCard * armorCardCount; // 장 수만큼 누적
            owner.statSheet[upStat].AddBuff(new StatModifier(upTotal, upOp));

            // === 감소 효과 ===
            var downStat = (StatType)GetEffectiveParam(downStatTypeIdx);
            var (downValPerCard, downOp) = GetBuffFromParamPreferSticker(downValueCoefIdx);
            int downTotal = -downValPerCard * armorCardCount; // 감소이므로 음수로 적용
            owner.statSheet[downStat].AddBuff(new StatModifier(downTotal, downOp));

            string upSym   = upOp   == BuffOperationType.Multiplicative ? "×" : "+";
            string downSym = downOp == BuffOperationType.Multiplicative ? "×" : "+";

            Debug.Log(
                $"<color=cyan>[WeightOfArmor]</color> '갑옷' {armorCardCount}장 → " +
                $"{upStat} {upSym}{upTotal}, {downStat} {downSym}{downTotal} 적용"
            );

            return true;
        }
    }
}
