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
        /// <summary>
        /// 전투가 시작될 때,이름에 '갑옷'이라는 단어가 들어간 카드 한 장 당 방어력이 10% 증가하고, 이동속도가 1% 감소합니다. 
        /// </summary>
        private const int upStatTypeIdx = 0;
        private const int upValueCoefIdx = 1;
        private const int downStatTypeIdx = 2;
        private const int downValueCoefIdx = 3;

        public Card0402_WeightOfArmor()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValueCoefIdx]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[downStatTypeIdx])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[downValueCoefIdx]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }
        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[ArmorWeightParam] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // "갑옷"이 이름에 포함된 카드 개수
                int armorCardCount = deck.SubstringCount("갑옷");

                if (armorCardCount == 0)
                {
                    Debug.Log("[ArmorWeightParam] '갑옷'이 포함된 카드 없음.");
                    return false;
                }

                var upStat = (StatType)GetEffectiveParam(upStatTypeIdx);
                int upValuePercent = Convert.ToInt32(GetEffectiveParam(upValueCoefIdx))*armorCardCount;
                var downStat = (StatType)GetEffectiveParam(downStatTypeIdx);
                int downValuePercent = Convert.ToInt32(GetEffectiveParam(downValueCoefIdx))*armorCardCount;

                owner.statSheet[upStat].AddBuff(new StatModifier(upValuePercent, BuffOperationType.Multiplicative));
                owner.statSheet[downStat].AddBuff(new StatModifier(-1*downValuePercent, BuffOperationType.Multiplicative));

                Debug.Log($"<color=cyan>[ArmorWeightParam] '{owner.gameObject.name}' - '갑옷' 카드 {armorCardCount}장: {upStat} ×{upValuePercent:F3}, {downStat} ×{downValuePercent:F3} 적용</color>");
                return true;
            }

            return false;
        }
    }
}