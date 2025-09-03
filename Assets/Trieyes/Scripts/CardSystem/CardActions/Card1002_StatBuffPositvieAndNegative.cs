using Stats;
using System.Collections.Generic;
using CharacterSystem;
using CardSystem;
using Utils;
using System;
using UnityEngine;

namespace CardActions
{
    public class Card1002_StatBuffPositiveAndNegative : CardAction
    {
        private readonly bool isMultiplicative;
        private readonly int pairCount;
        public Card1002_StatBuffPositiveAndNegative(bool isMultiplicative = false)
        {
            this.isMultiplicative = isMultiplicative;
            this.pairCount = 2;
            actionParams = new List<ActionParam>();

            for (int i = 0; i < 2; i++)
            {
                int statIdx = i * 2;
                int valueIdx = i * 2 + 1;
                // [0] 스탯 타입
                actionParams.Add(ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIdx])
                ));
                // [1] 수치(레벨 곱 적용)
                actionParams.Add(ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[valueIdx]);
                    return baseValue * card.cardEnhancement.level.Value;
                }));
            }
        }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning($"[GenericStatBuffOnBattleStartAction] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                for (int i = 0; i < pairCount; i++)
                {
                    // StatType과 Value를 각각 치환 적용
                    var statType = (StatType)GetEffectiveParam(i * 2);
                    int value = Convert.ToInt32(GetEffectiveParam(i * 2 + 1));

                    if (i == 0) value *= -1;

                    BuffOperationType op = isMultiplicative ? BuffOperationType.Multiplicative : BuffOperationType.Additive;
                    owner.statSheet[statType].AddBuff(new StatModifier(value, op));

                    string opStr = isMultiplicative ? "Multiplicative(곱연산)" : "Additive(합연산)";
                    Debug.Log($"[GenericStatBuff] {statType} {(isMultiplicative ? "×" : "+")}{value} ({opStr}), 현재값: {owner.statSheet[statType].Value}");
                }
                return true;
            }

            return false;
        }
    }
}
