using UnityEngine;
using UnityEngine.UI;
using CardSystem;

namespace UI
{
    public class DeckEventTester : MonoBehaviour
    {
        public Deck deck;                // Inspector에서 연결
        public Button battleStartButton; // "전투 시작" 버튼
        public Button battleEndButton;   // "전투 종료" 버튼
        public Button purchaseButton;    // "카드 구매" 버튼 (optional)

        // 테스트용 카드(Inspector에서 연결)
        public Card purchaseCard;        

        private void Awake()
        {
            // 버튼마다 이벤트 등록
            if (battleStartButton != null)
                battleStartButton.onClick.AddListener(OnBattleStart);

            if (battleEndButton != null)
                battleEndButton.onClick.AddListener(OnBattleEnd);

            if (purchaseButton != null)
                purchaseButton.onClick.AddListener(OnCardPurchase);
        }

        // 버튼에서 호출할 메서드들
        public void OnBattleStart()
        {
            if (deck != null)
            {
                Debug.Log("<color=lime>[DeckTester] 전투 시작!</color>");
                deck.OnEvent(Utils.EventType.OnBattleStart, null);
            }
        }

        public void OnBattleEnd()
        {
            if (deck != null)
            {
                Debug.Log("<color=red>[DeckTester] 전투 종료!</color>");
                deck.OnEvent(Utils.EventType.OnBattleEnd, null);
            }
        }

        public void OnCardPurchase()
        {
            if (deck != null && purchaseCard != null)
            {
                Debug.Log("<color=yellow>[DeckTester] 카드 구매 시뮬레이션!</color>");
                deck.OnEvent(Utils.EventType.OnCardPurchase, purchaseCard);
            }
        }
    }
}