using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using UnityEngine.UI;

namespace DeckViews
{
    public class DeckZoneManager : MonoBehaviour
    {
        public Transform deckZone;
        public GameObject cardPrefab;
        public Button removeButton;

        private List<GameObject> cardViews = new();
        private CardInDeckView selectedCard1;
        private CardInDeckView selectedCard2;
        private Deck currentDeck;
    
        public static DeckZoneManager Instance {private set; get;}
    
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
        // 덱 UI 갱신
        public void RefreshDeckUI(Deck deck)
        {
            currentDeck = deck;
            // 기존 카드 오브젝트 제거
            foreach (var obj in cardViews)
                Destroy(obj);
            cardViews.Clear();

            // 덱에 있는 카드 수 만큼 프리팹 인스턴스화
            foreach (var card in deck.Cards)
            {
                var cardViewInstance = Instantiate(cardPrefab, deckZone);
                // 카드 정보 반영 (CardView 스크립트의 SetCard 활용)
                cardViewInstance.GetComponent<CardViews.CardView>().SetCard(card);
                cardViews.Add(cardViewInstance);
            }
        }

        public void OnCardClicked(CardInDeckView cardView)
        {
            // 첫번째 카드가 선택되지 않은 경우
            if (selectedCard1 == null)
            {
                selectedCard1 = cardView;
                selectedCard1.SetSelected(true);
                removeButton.interactable = true; // 첫 카드만 선택하면 제거 버튼 표시
                return;
            }
            
            // 이미 선택된 카드 다시 누르면 해제
            else if (selectedCard1 == cardView)
            {
                selectedCard1.SetSelected(false);
                selectedCard1 = null;
                removeButton.interactable = false;
                return;
            }

            // 두번째 카드 선택 (그리고 두 카드가 다를 때만)
            else if (selectedCard1 != cardView)
            {
                selectedCard2 = cardView;
                selectedCard2.SetSelected(true);

                // 덱에서 카드 스왑!
                if(currentDeck is null) Debug.Log("현재 덱이 설정되지 않았습니다.");
                currentDeck.SwapCards(selectedCard1.card, selectedCard2.card);

                // 둘 다 선택 해제
                selectedCard1.SetSelected(false);
                selectedCard2.SetSelected(false);
                selectedCard1 = null;
                selectedCard2 = null;
                removeButton.interactable = false;

                // UI 새로고침
                RefreshDeckUI(currentDeck);
            }
        }

        public void OnRemoveButtonClicked()
        {
            if (selectedCard1 == null) return;
            currentDeck.RemoveCard(selectedCard1.card);
            selectedCard1.SetSelected(false);
            selectedCard1 = null;
            removeButton.interactable = false;
            RefreshDeckUI(currentDeck);
        }
    }
}