using UnityEngine;
using CharacterSystem;
using CardSystem;
using System.Collections.Generic;
using System;

namespace CardActions
{
    /// <summary>
    /// 그림자(Shadow) 카드 액션을 구현하는 클래스입니다.
    /// CalcActionInitOrder 이벤트 발생 시 자기 자신을 제외한 다른 모든 카드를 지정된 횟수만큼 더 호출합니다.
    /// </summary>
    public class Shadow : CardAction
    {
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            // 호출 순서 재조정 이벤트 감지
            if (eventType == Utils.EventType.CalcActionInitOrder)
            {
                if (param is ValueTuple<Card, int> tuple)
                {
                    Card card = tuple.Item1;
                    int level = card.cardEnhancement.level;
                    int currentCardIndex = tuple.Item2;

                    HandleCalcActionInitOrder(deck, level, currentCardIndex);
                }
                else
                {
                    Debug.LogError("[Shadow] param이 올바르지 않음");
                }
            }
        }

        public int calRepeatCount(int cardLevel)
        {
            return cardLevel;
        }

        /// <summary>
        /// 자기 자신을 제외한 다른 모든 카드를 'repeatCount'번 추가 호출.
        /// param은 string[](descParams) 또는 int로 전달될 수 있음.
        /// </summary>
        private void HandleCalcActionInitOrder(Deck deck, int cardLevel, int currentCardIndex)
        {
            int repeatCount = calRepeatCount(cardLevel); // 기본값

            // 덱에 카드가 1개 이하인 경우 효과 없음
            if (deck.Cards.Count <= 1 || currentCardIndex < 0)
            {
                Debug.Log("<color=yellow>[Shadow] Only one card in deck or invalid index, no effect</color>");
                return;
            }

            List<int> cardsToAppend = new List<int>();
            List<int> callOrder = deck.GetCallOrder();

            // 자기 자신을 제외한 카드 인덱스를 repeatCount번 추가
            for (int repeat = 0; repeat < repeatCount; repeat++)
            {
                for (int i = 0; i < deck.Cards.Count; i++)
                {
                    if (i != currentCardIndex)
                        cardsToAppend.Add(i);
                }
            }

            callOrder.AddRange(cardsToAppend);

            Debug.Log($"<color=green>[Shadow] {deck.GetOwner().gameObject.name} appended other cards {repeatCount}x (excluding {currentCardIndex}): [{string.Join(", ", cardsToAppend)}]</color>");
        }
        
        public override string[] GetDescriptionParams(Card card)
        {
            int repeatCount = calRepeatCount(card.cardEnhancement.level.Value);
            return new string[] { repeatCount.ToString() };
        }
    }
}
