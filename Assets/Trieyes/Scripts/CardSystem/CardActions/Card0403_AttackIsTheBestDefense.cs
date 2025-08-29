using System.Collections.Generic;
using System.Linq;
using Stats;
using Utils;
using CharacterSystem;
using CardSystem;
using UnityEngine;
using System;

namespace CardActions
{
    public class Card0403_AttackIsTheBestDefense : CardAction
    {
        /// <summary>
        /// desc: 전투가 시작될 때, ‘강철’ 속성 카드 한 장 당 공격력이  5% 증가합니다.
        /// </summary>
        private const int statIndex = 0;
        private const int valueIndex = 1;

        public Card0403_AttackIsTheBestDefense()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIndex])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[valueIndex]);
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
                // "강철"이 이름에 포함된 카드 개수
                int steelCardCount = deck.PropertyCount(Property.Steel);

                if (steelCardCount == 0)
                {
                    Debug.Log("[ArmorWeightParam] '갑옷'이 포함된 카드 없음.");
                    return false;
                }

                var stat = (StatType)GetEffectiveParam(statIndex);
                int valuePercent = Convert.ToInt32(GetEffectiveParam(valueIndex)) * steelCardCount;

                owner.statSheet[stat].AddBuff(new StatModifier(valuePercent, BuffOperationType.Multiplicative));
                return true;
            }

            return false;
        }
    }
}