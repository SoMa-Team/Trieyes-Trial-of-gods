using CharacterSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;
using CardViews;

namespace NodeStage
{
    public class BattleRewardStage : EventStage<BattleRewardStage>
    {
        [Header("보상 프리팹/컨테이너")]
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private GoldRewardView goldRewardPrefab;

        [Header("컨트롤 UI")]
        [SerializeField] private Button nextButton;

        [Header("보상 설정값")]
        [SerializeField] private int cardChoiceCount = 3;
        [SerializeField] private int goldAmount = 20;

        private readonly List<CardView> cardSlots = new();
        private GoldRewardView goldView;
        private CardView selectedCard;
        private bool goldSelected;

        protected override void OnActivated()
        {
            if (nextButton) nextButton.interactable = false;
            SetupRewards();
        }

        protected override void OnDeactivated()
        {
            ClearRewards();
        }

        private void SetupRewards()
        {
            ClearRewards();

            for (int i = 0; i < cardChoiceCount; i++)
            {
                var cv = Instantiate(cardPrefab, rewardContainer);
                cv.SetCard(CardFactory.Instance.RandomCreate());
                cv.SetCanInteract(true);
                cv.SetSelected(false);
                cv.SetOnClicked(OnCardClicked);
                cardSlots.Add(cv);
            }

            if (goldRewardPrefab != null)
            {
                goldView = Instantiate(goldRewardPrefab, rewardContainer);
                goldView.Activate(goldAmount, OnGoldClicked);
            }
        }

        private void ClearRewards()
        {
            foreach (var cv in cardSlots) if (cv) Destroy(cv.gameObject);
            cardSlots.Clear();
            if (goldView != null) { Destroy(goldView.gameObject); goldView = null; }
            selectedCard = null;
            goldSelected = false;
            if (nextButton) nextButton.interactable = false;
        }

        private void OnCardClicked(CardView clicked)
        {
            if (goldSelected && goldView != null) { goldView.SetSelected(false); goldSelected = false; }

            if (selectedCard == clicked)
            {
                clicked.SetSelected(false);
                selectedCard = null;
            }
            else
            {
                if (selectedCard != null) selectedCard.SetSelected(false);
                selectedCard = clicked;
                selectedCard.SetSelected(true);
            }
            if (nextButton) nextButton.interactable = (selectedCard != null) || goldSelected;
        }

        private void OnGoldClicked(GoldRewardView view)
        {
            if (selectedCard != null) { selectedCard.SetSelected(false); selectedCard = null; }

            goldSelected = (goldView == view) && !goldSelected;
            if (goldView != null) goldView.SetSelected(goldSelected);
            if (nextButton) nextButton.interactable = (selectedCard != null) || goldSelected;
        }

        public override void NextStage()
        {
            if (selectedCard != null)
            {
                var picked = selectedCard.GetCurrentCard().DeepCopy();
                mainCharacter.deck.AddCard(picked);
            }
            else if (goldSelected && goldView != null)
            {
                mainCharacter.gold += goldView.Amount;
            }

            base.NextStage(); // ✅ 공통 전환
        }

        // 덱 버튼은 부모(OpenDeckInspectOnly)로 처리됨
    }
}
