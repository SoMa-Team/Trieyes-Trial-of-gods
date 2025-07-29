using System.Collections.Generic;
using Utils;
using UnityEngine;
using Stats;
using CardSystem;
using CharacterSystem;
using System;

namespace CardActions
{
    public class Card0602_Berserker : CardAction
    {
        private const int upStatTypeIdx = 0;
        private const int upValueCoefIdx = 1;

        private StatModifier stat1Modifier;

        public Card0602_Berserker()
        {
            actionParams = new List<ActionParam>
            {
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
                stat1Modifier = new StatModifier(0, BuffOperationType.Additive, false);
            }

            if (eventType == Utils.EventType.OnDamaged)
            {
                var statType = (StatType)GetEffectiveParam(upStatTypeIdx);
                
                stat1Modifier.value+=(int)GetEffectiveParam(upValueCoefIdx);
                
                owner.statSheet[statType].AddBuff(stat1Modifier);
            }
        }
    }
}