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

        private StatModifier stat1Modifier;

        public Card0802_RageOfBlade()
        {
            actionParams = new List<ActionParam>
            {
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[downValueCoefIdx];
                    int.TryParse(raw, out int baseValue);
                    return baseValue;
                }),
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[upStatTypeIdx])),
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[upValueCoefIdx];
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
                stat1Modifier = new StatModifier(0, BuffOperationType.Multiplicative, false);
                
                var statType = (StatType)GetEffectiveParam(downStatTypeIdx);
                var value = Convert.ToInt32(GetEffectiveParam(downValueCoefIdx)) * -1;
                
                owner.statSheet[statType].AddBuff(new StatModifier(value, BuffOperationType.Multiplicative));
            }

            if (eventType == Utils.EventType.OnAttack)
            {
                var statType = (StatType)GetEffectiveParam(upStatTypeIdx);
                
                stat1Modifier.value+=(int)GetEffectiveParam(upValueCoefIdx);
                
                owner.statSheet[statType].AddBuff(stat1Modifier);
            }
        }
    }
}