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
    public class Card1003_StatProbabilityBuff : CardAction
    {
        private readonly int pairCount;

        /// <param name="numPairs">몇 쌍의 (스탯, 값) 세트인지 (ex. 1, 2, 3, ...)</param>
        public Card1003_StatProbabilityBuff(int numPairs)
        {
            this.pairCount = numPairs;
            actionParams = new List<ActionParam>();

            for (int i = 0; i < numPairs; i++)
            {
                int statIdx = i * 2;
                int valueIdx = i * 2 + 1;
                // [0] 스탯 타입
                actionParams.Add(ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIdx])
                ));
                // [1] 수치(레벨 곱 적용)
                actionParams.Add(ActionParamFactory.Create(ParamKind.Probability, card =>
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
                    
                    owner.statSheet[statType].AddBuff(new StatModifier(value, BuffOperationType.Additive));
                }
                return true;
            }
            return false;
        }
    }
}