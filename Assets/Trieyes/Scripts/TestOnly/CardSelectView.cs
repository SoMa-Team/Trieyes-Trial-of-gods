using UnityEngine;
using UnityEngine.UI;
using CardSystem;
using CardViews;

namespace OutGame{
    public class CardSelectView : MonoBehaviour
    {
        public CardSelectListView cardSelectListView;

        public Button button;
        public Card Card;

        public CardView cardView;
        
        // 초기화 메서드
        public void Awake()
        {
            button = GetComponent<Button>();
            cardView = GetComponent<CardView>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnButtonClicked());
            }

            Card = CardFactory.Instance.RandomCreate();
            cardView.SetCard(Card);
        }

        public void SetCardSelectListView(CardSelectListView cardSelectListView)
        {
            this.cardSelectListView = cardSelectListView;
        }

        public void SetCardView(CardView cardView)
        {
            this.cardView = cardView;
        }

        public void SetSelectedCard(Card selectedCard)
        {
            cardSelectListView.selectedCard = selectedCard;
        }

        public void OnButtonClicked()
        {
            Debug.Log("카드 설명 : " + cardView.GetCurrentCard().cardDescription);
            cardSelectListView.selectedCard = Card;
        }
    }
}