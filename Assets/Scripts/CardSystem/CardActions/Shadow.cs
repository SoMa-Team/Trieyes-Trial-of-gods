using UnityEngine;
using CharacterSystem;
using CardSystem;
using System.Collections.Generic;

namespace CardActions
{
    [CreateAssetMenu(menuName = "CardActions/Shadow")]
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
                HandleCalcActionInitOrder(deck, param);
            }
        }

        /// <summary>
        /// 자기 자신을 제외한 다른 모든 카드를 한 번 더 호출하는 로직
        /// </summary>
        /// <param name="deck">현재 덱</param>
        /// <param name="param">현재 카드의 인덱스</param>
        private void HandleCalcActionInitOrder(Deck deck, object param)
        {
            if (param is int currentCardIndex)
            {
                if (deck.Cards.Count <= 1)
                {
                    Debug.Log("<color=yellow>[Shadow] Only one card in deck, no effect</color>");
                    return;
                }

                // 자기 자신을 제외한 카드 인덱스를 수집
                List<int> cardsToAppend = new List<int>();
                List<int> callOrder = deck.GetCallOrder();

                for (int i = 0; i < deck.Cards.Count; i++)
                {
                    if (i != currentCardIndex)
                        cardsToAppend.Add(i);
                }

                // 기존 순서에 추가
                callOrder.AddRange(cardsToAppend);

                Debug.Log($"<color=green>[Shadow] {deck.GetOwner().gameObject.name} appended other cards once (excluding {currentCardIndex}): [{string.Join(", ", cardsToAppend)}]</color>");
            }
        }
    }
}
