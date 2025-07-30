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
        private const int upStat1Index = 0;
        private const int upValue1Index = 1;
        private const int upStat2Index = 2;
        private const int upValue2Index = 3;
        
        private const int duration = 3;
        
        private StatModifier stat1Modifier;
        private StatModifier stat2Modifier;

        public Card0404_KillingSpree()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStat1Index])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValue1Index]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStat2Index])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseValue = Parser.ParseStrToInt(card.baseParams[upValue2Index]);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }
        
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning($"[GenericStatBuffOnBattleStartAction] owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)   
            {
                stat1Modifier = new StatModifier((int)GetEffectiveParam(upValue1Index), BuffOperationType.Multiplicative, false, duration);
                stat2Modifier = new StatModifier((int)GetEffectiveParam(upValue2Index), BuffOperationType.Multiplicative, false, duration);
            }

            if (eventType == Utils.EventType.OnKilled)
            {
                var statType1 = (StatType)GetEffectiveParam(upStat1Index);
                var statType2 = (StatType)GetEffectiveParam(upStat2Index);
                
                stat1Modifier.endTime = BattleStage.now.GetTime() + duration;
                stat2Modifier.endTime = BattleStage.now.GetTime() + duration;
                
                owner.statSheet[statType1].AddBuff(stat1Modifier);
                owner.statSheet[statType2].AddBuff(stat2Modifier);
            }
        }
    }
}