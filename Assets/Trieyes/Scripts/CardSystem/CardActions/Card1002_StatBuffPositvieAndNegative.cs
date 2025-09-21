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
        private readonly int pairCount = 2;

        /// <param name="numericKindDefault">
        /// 값 파라미터의 '기본' Kind. (스티커가 있으면 그 타입으로 덮어씀)
        /// </param>
        public Card1002_StatBuffPositiveAndNegative(ParamKind numericKindDefault = ParamKind.Add)
        {
            if (numericKindDefault == ParamKind.StatType)
                throw new ArgumentException("numericKindDefault cannot be StatType.");

            actionParams = new List<ActionParam>();

            for (int i = 0; i < pairCount; i++)
            {
                int statIdx  = i * 2;
                int valueIdx = i * 2 + 1;

                // [0] 스탯 타입
                actionParams.Add(ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIdx])
                ));

                // [1] 값(레벨 반영) — 기본 Kind는 Add/Percent 중 하나
                actionParams.Add(ActionParamFactory.Create(numericKindDefault, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[valueIdx]);
                    return baseValue * card.cardEnhancement.level.Value; // 기본값은 레벨 스케일
                }));
            }
        }

        // 하위호환: 기존 bool isMultiplicative 플래그 지원
        public Card1002_StatBuffPositiveAndNegative(bool isMultiplicative)
            : this(isMultiplicative ? ParamKind.Percent : ParamKind.Add) { }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (eventType != Utils.EventType.OnBattleSceneChange) return false;
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[Card1002] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            for (int i = 0; i < pairCount; i++)
            {
                // 스탯 타입: 스티커가 있으면 교체, 없으면 기본
                var statType = (StatType)GetEffectiveParam(i * 2);

                // 값 + 연산 타입: 스티커가 있으면 스티커(Add/Percent) 기준, 없으면 ParamKind 기준
                var (value, op) = GetBuffFromParamPreferSticker(i * 2 + 1);

                // 첫 번째 쌍은 음수(디버프)로 적용
                if (i == 0) value = -value;

                owner.statSheet[statType].AddBuff(new StatModifier(value, op));

                string opStr = op == BuffOperationType.Multiplicative ? "Multiplicative(곱연산)" : "Additive(합연산)";
                Debug.Log($"[Card1002] {statType} {(op == BuffOperationType.Multiplicative ? "×" : "+")}{value} ({opStr}), 현재값: {owner.statSheet[statType].Value}");
            }
            return true;
        }
    }
}
