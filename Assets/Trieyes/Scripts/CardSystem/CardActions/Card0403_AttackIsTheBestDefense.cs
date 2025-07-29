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
        private const int statIndex = 0;
        private const int valueIndex = 1;

        public Card0403_AttackIsTheBestDefense()
        {
            actionParams = new List<ActionParam>
            {
                // [0] 공격력 스탯 타입
                ActionParamFactory.Create(ParamKind.StatType, card =>
                    StatTypeTransformer.KoreanToStatType(card.baseParams[statIndex])),
                // [1] 올라가는 수치 계수 (예: 10)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    string raw = card.baseParams[valueIndex];
                    int.TryParse(raw, out int baseValue);
                    return baseValue * card.cardEnhancement.level.Value;
                }),
            };
        }
        
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[ArmorWeightParam] owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                // "강철"이 이름에 포함된 카드 개수
                int steelCardCount = 0;
                foreach (var card in deck.Cards)
                {
                    if (card == null || string.IsNullOrEmpty(card.cardName)) continue;
                    if (card.properties.Contains(Property.Steel))
                        steelCardCount++;
                }

                if (steelCardCount == 0)
                {
                    Debug.Log("[ArmorWeightParam] '갑옷'이 포함된 카드 없음.");
                    return;
                }

                var stat = (StatType)GetEffectiveParam(statIndex);
                int valuePercent = Convert.ToInt32(GetEffectiveParam(valueIndex)) * steelCardCount;

                owner.statSheet[stat].AddBuff(new StatModifier(valuePercent, BuffOperationType.Multiplicative));
            }
        }
    }
}