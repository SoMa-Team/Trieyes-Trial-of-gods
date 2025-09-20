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
        private const int statIndex  = 0;
        private const int valueIndex = 1;

        public Card0403_AttackIsTheBestDefense()
        {
            actionParams = new List<ActionParam>
            {
                // 증가 대상 스탯
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIndex])),

                // 1장당 증가 값 (기본 Percent). 스티커(Add/Percent)로 연산 전환 가능
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[valueIndex]);
                    return baseValue * card.cardEnhancement.level.Value; // 기본값은 레벨 스케일
                }),
            };
        }
        
        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (eventType != Utils.EventType.OnBattleSceneChange) return false;
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[AttackIsTheBestDefense] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            // '강철' 속성 카드 개수
            int steelCardCount = deck.PropertyCount(Property.Steel);
            if (steelCardCount == 0)
            {
                Debug.Log("[AttackIsTheBestDefense] '강철' 속성 카드 없음.");
                return false;
            }

            var stat = (StatType)GetEffectiveParam(statIndex);

            // 스티커가 있으면 스티커 타입(Add/Percent) 우선, 없으면 ParamKind(Percent) 사용
            var (perCardValue, op) = GetBuffFromParamPreferSticker(valueIndex);

            int total = perCardValue * steelCardCount;
            owner.statSheet[stat].AddBuff(new StatModifier(total, op));

            string sym = op == BuffOperationType.Multiplicative ? "×" : "+";
            Debug.Log($"[AttackIsTheBestDefense] '강철' {steelCardCount}장 → {stat} {sym}{total}");

            return true;
        }
    }
}