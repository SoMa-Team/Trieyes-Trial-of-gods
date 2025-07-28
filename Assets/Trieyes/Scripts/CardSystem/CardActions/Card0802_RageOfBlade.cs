using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;

namespace CardActions
{
    public class Card0802_RageOfBlade : CardAction
    {
        private const int downStatTypeIdx = 0;
        private const int downValueCoefIdx = 1;
        private const int upStatTypeIdx = 2;
        private const int upValueCoefIdx = 3;
        
        private StatModifier attackSpeedModifier = new StatModifier(0, BuffOperationType.Multiplicative, false);

        public Card0802_RageOfBlade()
        {
            actionParams = new List<ActionParam>
            {
                // [0] 공격속도 스탯 타입
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                // [1] 내려가는 수치 계수
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[downValueCoefIdx];
                    int.TryParse(raw, out int baseValue);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
                // [2] 공격속도 스탯 타입
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                // [3] 올라가는 수치 계수
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[downValueCoefIdx];
                    int.TryParse(raw, out int baseValue);
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
                var statType = (StatType)GetEffectiveParam(downStatTypeIdx);
                var value = Convert.ToInt32(GetEffectiveParam(downValueCoefIdx)) * -1;
                
                owner.statSheet[statType].AddBuff(new StatModifier(value, BuffOperationType.Multiplicative));
            }

            if (eventType == Utils.EventType.OnAttack)
            {
                var statType = (StatType)GetEffectiveParam(upStatTypeIdx);
                var value = Convert.ToInt32(GetEffectiveParam(upValueCoefIdx));

                attackSpeedModifier.value++;
                
                owner.statSheet[statType].AddBuff(attackSpeedModifier);
            }
        }
    }
}