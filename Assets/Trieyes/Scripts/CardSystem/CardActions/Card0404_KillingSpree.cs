using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;
using BattleSystem;

namespace CardActions
{
    /// <summary>
    /// desc: 적을 처치할 때마다, 3초 동안 이동속도 가 20%만큼 증가하고, 공격속도 가 20%만큼 증가합니다. 이 효과는 중첩되지 않습니다.
    /// </summary>
    public class Card0404_KillingSpree : CardAction
    {
        private const int upStat1Index  = 0;
        private const int upValue1Index = 1;
        private const int upStat2Index  = 2;
        private const int upValue2Index = 3;

        private const int duration = 3;

        private StatModifier stat1Modifier;
        private StatModifier stat2Modifier;

        public Card0404_KillingSpree()
        {
            actionParams = new List<ActionParam>
            {
                // 버프 1: 스탯 / 값(기본 Percent)
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStat1Index])),
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValue1Index]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),

                // 버프 2: 스탯 / 값(기본 Percent)
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStat2Index])),
                ActionParamFactory.Create(ParamKind.Percent, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValue2Index]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[KillingSpree] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // 스티커가 있으면 스티커 타입(Add/Percent) 우선, 없으면 ParamKind(Percent)
                var (v1, op1) = GetBuffFromParamPreferSticker(upValue1Index);
                var (v2, op2) = GetBuffFromParamPreferSticker(upValue2Index);

                // 같은 StatModifier 인스턴스를 재사용해 중첩 방지
                stat1Modifier = new StatModifier(v1, op1, canStack: false, duration: duration);
                stat2Modifier = new StatModifier(v2, op2, canStack: false, duration: duration);
                return true;
            }

            if (eventType == Utils.EventType.OnKilled)
            {
                var statType1 = (StatType)GetEffectiveParam(upStat1Index);
                var statType2 = (StatType)GetEffectiveParam(upStat2Index);

                // 남은 시간을 3초로 연장(재시작), 같은 인스턴스 재사용으로 중첩 방지
                float now = BattleStage.now?.GetTime() ?? 0f;
                stat1Modifier.endTime = now + duration;
                stat2Modifier.endTime = now + duration;

                owner.statSheet[statType1].AddBuff(stat1Modifier);
                owner.statSheet[statType2].AddBuff(stat2Modifier);
                return true;
            }

            return false;
        }
    }
}