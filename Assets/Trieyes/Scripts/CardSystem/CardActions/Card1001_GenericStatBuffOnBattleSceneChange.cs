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
    /// 전투 시작 시, 여러 스탯에 원하는 방식(합/곱)으로 버프를 부여하는 범용 액션.
    /// baseParams 예시: [스탯1, 값1, 타입1, 스탯2, 값2, 타입2, ...]
    /// 타입: "Additive", "Multiplicative" (대소문자 구분 X)
    /// </summary>
    public class Card1001_GenericPositiveOnlyOnBattleStart : CardAction
    {
        private readonly int pairCount;

        // 모든 (스탯, 값) 쌍에 대해 값 파라미터의 '기본' Kind를 지정 (스티커가 있으면 덮어씀)
        public Card1001_GenericPositiveOnlyOnBattleStart(int numPairs, ParamKind numericKindDefault = ParamKind.Percent)
        {
            if (numericKindDefault == ParamKind.StatType)
                throw new ArgumentException("numericKindDefault cannot be StatType.");

            pairCount = numPairs;
            actionParams = new List<ActionParam>();

            for (int i = 0; i < numPairs; i++)
            {
                int statIdx  = i * 2;
                int valueIdx = i * 2 + 1;

                actionParams.Add(ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIdx])));

                actionParams.Add(ActionParamFactory.Create(numericKindDefault, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[valueIdx]);
                    return baseValue * card.cardEnhancement.level.Value; // 기본값은 레벨 스케일
                }));
            }
        }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (eventType != Utils.EventType.OnBattleSceneChange) return false;
            if (owner == null || deck == null) return false;

            for (int i = 0; i < pairCount; i++)
            {
                StatType stat = (StatType)GetEffectiveParam(i * 2);
                var (val, op) = GetBuffFromParamPreferSticker(i * 2 + 1);
                owner.statSheet[stat].AddBuff(new StatModifier(val, op));
            }
            return true;
        }
    }

}