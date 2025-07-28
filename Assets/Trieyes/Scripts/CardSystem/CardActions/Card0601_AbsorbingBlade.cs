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
    /// 전투 시작 전 오른쪽 카드를 파괴 & 경험치 일부 흡수,
    /// 전투 시작 시 두 스탯을 각각 %만큼 곱연산 버프!
    /// baseParams: [스탯1, %1, 스탯2, %2] (치환/스티커 지원)
    /// </summary>
    public class Card0601_AbsorbingBlade : Card1001_GenericPositiveOnlyOnBattleStart
    {
        private int levelUpValue = 5;
        public Card0601_AbsorbingBlade()
            : base(1, true) // 2쌍, Multiplicative
        {
            // 두 Value 파라미터만 30+5*레벨로 오버라이드
            // actionParams = [Stat0, Value0, Stat1, Value1]
            actionParams[1] = ActionParamFactory.Create(ParamKind.Number, card =>
            {
                string raw = card.baseParams[1];
                if (!int.TryParse(raw, out int baseValue))
                    throw new InvalidOperationException($"[GenericStatBuff] baseParams[{1}] 변환 실패: {raw}");
                return baseValue + levelUpValue * card.cardEnhancement.level.Value;
            });
        }

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 전투 시작 전: 오른쪽 카드 파괴 & 경험치 일부 흡수
            if (eventType == Utils.EventType.DestoryCardsBeforeBattleStart)
            {
                if (!(param is int myIdx))
                {
                    Debug.LogError("[Card0301] 카드 인덱스 param이 int가 아님!");
                    return;
                }
                if (myIdx < deck.Cards.Count - 1)
                {
                    var rightCard = deck.Cards[myIdx + 1];
                    int rightLevel = rightCard.cardEnhancement.level.Value;
                    deck.RemoveCard(rightCard);

                    // 경험치 스택: 파괴된 카드 레벨 * 5
                    card.cardEnhancement.AddExp(rightLevel * 5);
                    card.RefreshStats();

                    Debug.Log($"[Card0301] 오른쪽 카드 {rightCard.cardName}(레벨 {rightLevel}) 파괴됨, 경험치 +{rightLevel * 5}");
                }
                else
                {
                    Debug.Log("[Card0301] 오른쪽 카드가 없음 (파괴 불가)");
                }
            }

            // 전투 시작 시: 스탯 버프는 부모쪽에서 자동 적용됨
            base.OnEvent(owner, deck, eventType, param);
        }
    }
}
