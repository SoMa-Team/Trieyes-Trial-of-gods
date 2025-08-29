using UnityEngine;
using CharacterSystem;
using CardSystem;
using System.Collections.Generic;
using System;
using Utils;

namespace CardActions
{
    /// <summary>
    /// desc: 전투가 시작할 때 ,나 이외의 카드를 한 번 더 발동시킵니다.
    /// </summary>
    public class Card0401_Shadow : CardAction
    {
        private const int repeatCountIndex = 0;
        public Card0401_Shadow()
        {
            actionParams = new List<ActionParam>
            {
                // [0] 반복 횟수 (CSV 예: 1)
                ActionParamFactory.Create(ParamKind.Number, card =>
                {
                    int baseCount = Parser.ParseStrToInt(card.baseParams[repeatCountIndex]);
                    return baseCount * card.cardEnhancement.level.Value;
                })
            };
        }

        /// <summary>
        /// CalcActionInitOrder 이벤트에서 효과 발동.
        /// </summary>
        
        public override bool OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("[Shadow] owner 또는 deck이 정의되지 않았습니다.");
                return false;
            }

            if (eventType == Utils.EventType.CalcActionInitOrder)
            {
                // param: (Card, int) 튜플에서 자신의 카드 인덱스를 가져옴
                if (param is ValueTuple<Card, int> tuple)
                {
                    int currentCardIndex = tuple.Item2;
                    int repeatCount = Convert.ToInt32(GetEffectiveParam(repeatCountIndex));
                    HandleCalcActionInitOrder(deck, repeatCount, currentCardIndex);
                }
                else
                {
                    Debug.LogError("[Shadow] param 형식이 잘못되었습니다. (ValueTuple<Card, int>이어야 함)");
                }

                return true;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                
            }

            return false;
        }

        /// <summary>
        /// 자기 자신을 제외한 모든 카드의 인덱스를 repeatCount번 callOrder에 추가.
        /// </summary>
        private void HandleCalcActionInitOrder(Deck deck, int repeatCount, int currentCardIndex)
        {
            // 예외: 덱이 1장뿐이거나, 인덱스가 유효하지 않은 경우 무효
            if (deck.Cards.Count <= 1 || currentCardIndex < 0 || currentCardIndex >= deck.Cards.Count)
            {
                Debug.Log("<color=yellow>[Shadow] 유효하지 않은 상황: 카드가 한 장뿐이거나 인덱스가 잘못됨 (효과 없음)</color>");
                return;
            }

            var cardsToAppend = new List<int>();
            var callOrder = deck.GetCallOrder();

            for (int repeat = 0; repeat < repeatCount; repeat++)
            {
                for (int i = 0; i < deck.Cards.Count; i++)
                {
                    if (i != currentCardIndex)
                        cardsToAppend.Add(i);
                }
            }

            callOrder.AddRange(cardsToAppend);

            Debug.Log($"<color=green>[Shadow] {deck.GetOwner().gameObject.name}의 카드 효과: 자신({currentCardIndex}) 제외 {repeatCount}회 추가 발동 [{string.Join(", ", cardsToAppend)}]</color>");
        }
    }
}
