using CharacterSystem;
using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using CardViews;
using UnityEngine.UI;

namespace NodeStage
{
    public class StartCardStage : EventStage<StartCardStage>
    {
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private Button nextButton;

        private readonly List<CardView> cardSlots = new();
        private CardView selected;
        [SerializeField] private int defaultCardCount = 3;

        protected override void OnActivated()
        {
            SetupCardSlots();
            if (nextButton) nextButton.interactable = false;
        }

        protected override void OnDeactivated()
        {
            ClearSlots();
        }

        private void SetupCardSlots()
        {
            ClearSlots();
            for (int i = 0; i < defaultCardCount; i++)
            {
                var cv = Instantiate(cardPrefab, cardContainer);
                cv.SetCard(CardFactory.Instance.RandomCreate());
                cv.SetOnClicked(OnCardClicked);
                cardSlots.Add(cv);
            }
        }

        private void ClearSlots()
        {
            foreach (var cv in cardSlots) if (cv) Destroy(cv.gameObject);
            cardSlots.Clear();
            selected = null;
            if (nextButton) nextButton.interactable = false;
        }

        private void OnCardClicked(CardView clicked)
        {
            if (selected == clicked)
            {
                clicked.SetSelected(false);
                selected = null;
            }
            else
            {
                if (selected != null) selected.SetSelected(false);
                selected = clicked;
                selected.SetSelected(true);
            }
            if (nextButton) nextButton.interactable = (selected != null);
        }

        public override void NextStage()
        {
            if (selected != null)
            {
                var picked = selected.GetCurrentCard().DeepCopy();
                mainCharacter.deck.AddCard(picked);
            }
            base.NextStage();
        }
    }
}
