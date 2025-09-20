using Utils;
using System.Collections.Generic;
using UnityEngine;
using Stats;
using CharacterSystem;
using CardSystem;
using System;

namespace CardActions
{
    /// <summary>
    /// desc: 전투가 시작하기 전, 내 오른쪽에 있는 카드를 파괴하고 해당 카드의 경험치 일부를 얻습니다. 전투가 시작할 때, 해당 전투 동안 흡혈을/를 10%만큼 증가시킵니다.
    /// </summary>
    public class Card0601_AbsorbingBlade : Card1001_GenericPositiveOnlyOnBattleStart
    {
        private int levelUpValue = 5;

        // 부모: (numPairs=1, numericKindDefault=Percent)
        public Card0601_AbsorbingBlade()
            : base(1, ParamKind.Percent)
        {
            // 값 파라미터(인덱스 1)는 Percent로 선언.
            // 기본값: baseParams[1] + (levelUpValue * 카드 레벨)
            // (스티커가 붙으면 값/연산타입은 스티커가 우선)
            actionParams[1] = ActionParamFactory.Create(ParamKind.Percent, card =>
            {
                int baseValue = Parser.ParseStrToInt(card.baseParams[1]);
                return baseValue + levelUpValue * card.cardEnhancement.level.Value;
            });
        }

        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 전투 시작 전: 오른쪽 카드 파괴 및 경험치 획득
            if (eventType == Utils.EventType.DestoryCardsBeforeBattleStart)
            {
                if (param is not int myIdx)
                {
                    Debug.LogError("[Card0601] 카드 인덱스 param이 int가 아님!");
                    return false;
                }

                if (myIdx < deck.Cards.Count - 1)
                {
                    var rightCard  = deck.Cards[myIdx + 1];
                    int rightLevel = rightCard.cardEnhancement.level.Value;

                    deck.RemoveCard(rightCard);

                    // TODO: 디자인 의도대로 경험치/레벨 반영 로직 결정
                    // 현재: 레벨 +1 (로그는 rightLevel * 5로 안내)
                    card.cardEnhancement.level.AddToBasicValue(1);

                    Debug.Log($"[Card0601] 오른쪽 카드 {rightCard.cardName}(레벨 {rightLevel}) 파괴됨, 경험치 +{rightLevel * 5}");
                    return true;
                }
                else
                {
                    Debug.Log("[Card0601] 오른쪽 카드가 없음 (파괴 불가)");
                    return false;
                }
            }

            // 전투 시작 시: 스탯 버프는 부모(Card1001_... )에서 처리(스티커 우선 적용)
            base.OnEvent(owner, deck, eventType, param);
            return false;
        }
    }
}
