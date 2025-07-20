using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using UnityEngine.UI;
using CardViews;

namespace DeckViews
{
    /// <summary>
    /// 덱에 속한 카드의 UI 뷰와 사용자 인터랙션을 관리
    /// </summary>
    public class DeckView : MonoBehaviour
    {
        public Transform deckZone;
        public GameObject cardPrefab;
        public Button removeButton;

        private readonly List<CardView> cardViewsInDeck = new();
        private CardView selectedCard1;
        private CardView selectedCard2;
        private Deck currentDeck;

        private void Awake()
        {
            removeButton.onClick.AddListener(OnRemoveButtonClicked);
        }

        public void SetDeck(Deck deck)
        {
            currentDeck = deck;
            currentDeck.setDeckView(this);
            RefreshDeckUI();
        }

        public void RefreshDeckUI()
        {
            foreach (var view in cardViewsInDeck)
                Destroy(view.gameObject);
            cardViewsInDeck.Clear();

            if (currentDeck == null) return;

            foreach (var card in currentDeck.Cards)
            {
                var obj = Instantiate(cardPrefab, deckZone);
                var cardView = obj.GetComponent<CardView>();
                cardView.SetCard(card);
                cardView.SetParentDeckView(this); // 뷰 계층 구조 연결
                cardViewsInDeck.Add(cardView);
            }
        }

        /// <summary>
        /// CardView가 클릭되었을 때 호출됨
        /// </summary>
        public void OnCardClicked(CardView cardView)
        {
            if (selectedCard1 == null)
            {
                selectedCard1 = cardView;
                selectedCard1.SetSelected(true);
                removeButton.interactable = true;
                return;
            }
            else if (selectedCard1 == cardView)
            {
                selectedCard1.SetSelected(false);
                selectedCard1 = null;
                removeButton.interactable = false;
                return;
            }
            else
            {
                selectedCard2 = cardView;
                selectedCard2.SetSelected(true);

                if (currentDeck == null) return;

                // 병합 or 스왑
                var cardA = selectedCard1.GetCurrentCard();
                var cardB = selectedCard2.GetCurrentCard();

                if (cardA.cardName == cardB.cardName)
                    currentDeck.MergeCards(cardA, cardB);
                else
                    currentDeck.SwapCards(cardA, cardB);

                // 선택 해제
                selectedCard1.SetSelected(false);
                selectedCard2.SetSelected(false);
                selectedCard1 = null;
                selectedCard2 = null;
                removeButton.interactable = false;
            }
            RefreshDeckUI();
        }

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
