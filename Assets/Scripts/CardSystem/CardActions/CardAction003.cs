using UnityEngine;
using CharacterSystem;
using System.Collections.Generic;

namespace CardActions
{
    /// <summary>
    /// CardAction003: 자기 자신을 제외한 다른 모든 카드를 다시 한 번 호출하는 카드 액션
    /// 예: 카드가 6장이고 이 카드가 5번째에 있다면
    /// 호출 순서: 0->1->2->3->4->5->0->1->2->3->4->5
    /// </summary>
    public class CardAction003 : CardAction
    {
        [Header("CardAction003 Settings")]
        [SerializeField] private int cardId003 = 3;
        [SerializeField] private new string cardDescription = "다른 모든 카드를 다시 한 번 호출합니다";

        private void Awake()
        {
            Activate();
        }

        private void OnDestroy()
        {
            Deactivate();
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public virtual void Activate()
        {
            cardId = cardId003;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public virtual void Deactivate()
        {
            // CardAction003 고유 정리 로직
            cardId = 0;
        }

        // ===== [기능 1] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param); // 부모 클래스의 OnEvent 호출

            if (eventType == Utils.EventType.CalcActionInitOrder)
            {
                HandleCalcActionInitOrder(param);
            }
        }

        // ===== [기능 2] 호출 순서 계산 요청 =====
        /// <summary>
        /// CalcActionInitOrder 이벤트를 처리하여 Deck에 호출 순서 조정을 요청합니다.
        /// 자기 자신을 제외한 다른 모든 카드를 다시 한 번 호출하도록 요청합니다.
        /// </summary>
        /// <param name="param">현재 카드의 인덱스</param>
        private void HandleCalcActionInitOrder(object param)
        {
            if (param is int currentCardIndex)
            {
                if (deck.GetCurrentDeckSize() <= 1)
                {
                    Debug.Log("<color=yellow>[CardAction003] Only one card in deck, no effect</color>");
                    return;
                }

                // 제외할 카드를 제외한 다른 모든 카드들을 한 번 더 추가
                List<int> cardsToAppend = new List<int>();
                List<int> callOrder = deck.GetCallOrder(); // Deck의 호출 순서 리스트 참조를 가져옵니다.
            
                for (int i = 0; i < deck.GetCurrentDeckSize(); i++)
                {
                    if (i != currentCardIndex) // 자기 자신 제외
                    {
                        cardsToAppend.Add(i);
                    }
                }
                // 기존 순서에 덧붙여서 리스트를 직접 수정합니다.
                callOrder.AddRange(cardsToAppend);
            
                Debug.Log($"<color=green>[CardAction003] {deck.GetOwner().gameObject.name} appended other cards once (excluding {currentCardIndex}): [{string.Join(", ", cardsToAppend)}]</color>");
            }
        }
    }
} 