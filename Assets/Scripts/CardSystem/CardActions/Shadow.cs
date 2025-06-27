using UnityEngine;
using CharacterSystem;
using CardSystem;
using System.Collections.Generic;

namespace CardActions
{
    /// <summary>
    /// 그림자(Shadow) 카드 액션을 구현하는 클래스입니다.
    /// CalcActionInitOrder 이벤트 발생 시 자기 자신을 제외한 다른 모든 카드를 한 번 더 호출하는 특별한 효과를 가집니다.
    /// 카드 호출 순서를 조작하여 전략적 우위를 점하는 복잡한 카드입니다.
    /// </summary>
    public class Shadow : CardAction
    {
        // --- public 메서드 ---

        /// <summary>
        /// 카드 액션이 이벤트에 반응할 때 호출되는 메서드입니다.
        /// CalcActionInitOrder 이벤트 발생 시 호출 순서 재조정 로직을 실행합니다.
        /// 다른 카드들의 호출 순서를 조작하는 특별한 능력을 발동시킵니다.
        /// </summary>
        /// <param name="owner">카드를 소유한 캐릭터</param>
        /// <param name="deck">카드가 속한 덱</param>
        /// <param name="eventType">발생한 이벤트 타입</param>
        /// <param name="param">이벤트와 함께 전달된 매개변수</param>
        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            // 매개변수 유효성 검사
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

        // --- private 메서드 ---

        /// <summary>
        /// 자기 자신을 제외한 다른 모든 카드를 한 번 더 호출하는 로직을 처리합니다.
        /// 덱의 호출 순서에 다른 카드들의 인덱스를 추가하여 중복 호출을 구현합니다.
        /// 그림자 카드의 핵심 능력으로, 다른 카드들의 효과를 두 배로 증폭시킵니다.
        /// </summary>
        /// <param name="deck">현재 덱</param>
        /// <param name="param">현재 카드의 인덱스</param>
        private void HandleCalcActionInitOrder(Deck deck, object param)
        {
            if (param is int currentCardIndex)
            {
                // 덱에 카드가 1개 이하인 경우 효과 없음
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
