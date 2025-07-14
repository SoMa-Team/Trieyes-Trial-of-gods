using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using UnityEngine.UI;

namespace DeckViews
{
    /// <summary>
    /// 내 덱의 카드 UI 전체를 관리. 카드 추가/제거/스왑, UI 갱신 등 담당
    /// </summary>
    public class DeckZoneManager : MonoBehaviour
    {
        // 덱 카드들이 쌓일 부모 오브젝트(예: HorizontalLayoutGroup 등)
        public Transform deckZone;
        // 카드 프리팹(DeckCardView)
        public GameObject cardPrefab;
        // 카드 제거 버튼
        public Button removeButton;

        // 현재 표시 중인 카드 오브젝트들
        private List<GameObject> cardViewsInDeck = new();
        // 카드 선택 상태 관리(최대 2장까지)
        private DeckCardView selectedCard1;
        private DeckCardView selectedCard2;
        // 현재 표시 중인 덱 데이터
        private Deck currentDeck;

        // 싱글톤 인스턴스 (씬에 1개)
        public static DeckZoneManager Instance { private set; get; }

        /// <summary>
        /// 싱글톤 인스턴스 설정 및 removeButton 이벤트 연결
        /// </summary>
        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(this);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;

            removeButton.onClick.AddListener(OnRemoveButtonClicked);
        }

        public void setDeck(Deck deck)
        {
            currentDeck = deck;
        }

        /// <summary>
        /// 덱 데이터로부터 카드 UI를 모두 갱신
        /// </summary>
        public void RefreshDeckUI()
        {

            // 기존 카드 오브젝트 모두 제거
            foreach (var obj in cardViewsInDeck)
                Destroy(obj);
            cardViewsInDeck.Clear();

            // 덱에 있는 카드 수만큼 프리팹 인스턴스 생성 및 카드 정보 반영
            foreach (var card in currentDeck.Cards)
            {
                var cardViewInstance = Instantiate(cardPrefab, deckZone);
                cardViewInstance.GetComponent<CardViews.CardView>().SetCard(card);
                cardViewsInDeck.Add(cardViewInstance);
            }
        }

        /// <summary>
        /// 카드 클릭 시 호출(강조/선택/스왑/제거 UI 관리)
        /// </summary>
        public void OnCardClicked(DeckCardView cardView)
        {
            // [1] 아무것도 선택 안 했으면 첫 번째 카드 선택
            if (selectedCard1 == null)
            {
                Debug.Log("Card Selected");
                selectedCard1 = cardView;
                selectedCard1.SetSelected(true);
                removeButton.interactable = true; // 첫 카드만 선택하면 제거 버튼 표시
                return;
            }

            // [2] 이미 선택한 카드 다시 누르면 선택 해제
            else if (selectedCard1 == cardView)
            {
                selectedCard1.SetSelected(false);
                selectedCard1 = null;
                removeButton.interactable = false;
                return;
            }

            // [3] 다른 카드 누르면 두 번째 카드 선택 후, 같은 이름인지 확인
            else if (selectedCard1 != cardView)
            {
                selectedCard2 = cardView;
                selectedCard2.SetSelected(true);

                if(currentDeck is null) 
                {
                    Debug.Log("현재 덱이 설정되지 않았습니다.");
                    return;
                }

                // 같은 이름의 카드인지 확인
                if (selectedCard1.GetCurrentCard().cardName == selectedCard2.GetCurrentCard().cardName)
                {
                    // 카드 합치기 (Deck에서 처리)
                    currentDeck.MergeCards(selectedCard1.GetCurrentCard(), selectedCard2.GetCurrentCard());
                }
                else
                {
                    // 다른 이름이면 기존처럼 스왑
                    currentDeck.SwapCards(selectedCard1.GetCurrentCard(), selectedCard2.GetCurrentCard());
                }

                // 둘 다 선택 해제
                selectedCard1.SetSelected(false);
                selectedCard2.SetSelected(false);
                selectedCard1 = null;
                selectedCard2 = null;
                removeButton.interactable = false;
            }
            // UI 새로고침 (카드 순서 갱신)
            RefreshDeckUI();
        }

        /// <summary>
        /// removeButton 클릭 시 선택된 카드 덱에서 제거
        /// </summary>
        public void OnRemoveButtonClicked()
        {
            if (selectedCard1 == null) return;

            currentDeck.RemoveCard(selectedCard1.GetCurrentCard());
            selectedCard1.SetSelected(false);
            selectedCard1 = null;
            removeButton.interactable = false;
            RefreshDeckUI();
        }
    }
}
