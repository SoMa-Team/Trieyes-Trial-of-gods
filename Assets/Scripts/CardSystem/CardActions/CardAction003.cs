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
        [SerializeField] private string cardDescription = "다른 모든 카드를 다시 한 번 호출합니다";

        private void Awake()
        {
            cardId = cardId003;
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
                Debug.Log($"<color=cyan>[CardAction003] Requesting call order adjustment for card index: {currentCardIndex}</color>");
                
                // Deck에 호출 순서 조정 요청 - 자신을 제외한 다른 모든 카드를 한 번 더 호출
                deck.AppendOtherCardsOnce(currentCardIndex);
            }
        }
    }
} 