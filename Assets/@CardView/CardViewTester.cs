using UnityEngine;
using UnityEngine.UI;
using CardSystem;
using CardView;

namespace CardView
{
    public class CardViewTester : MonoBehaviour
    {
        public CardView cardView;
        public CardFactory cardFactory;
        
        public Button showPreparingMarch;
        public Button showCrouch;
        public Button showShadow;

        void Awake()
        {
            showPreparingMarch.onClick.AddListener(ShowPreparingMarch);
            showCrouch.onClick.AddListener(ShowCrouch);
            showShadow.onClick.AddListener(ShowShadow);
        }

        void ShowPreparingMarch()
        {
            Card card = cardFactory.Create(1, 0);
            cardView.SetCard(card);
        }

        void ShowCrouch()
        {
            Card card = cardFactory.Create(1, 1);
            cardView.SetCard(card);
        }
        
        void ShowShadow()
        {
            Card card = cardFactory.Create(1, 2);
            cardView.SetCard(card);
        }
    }
}