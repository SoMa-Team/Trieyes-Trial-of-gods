using CharacterSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;
using CardViews;
using GamePlayer;
using Stats;
using EventType = Utils.EventType;
using TMPro;

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
        
        private Card pendingRewardCard;
        private bool waitingForDeckChoice;

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
            
            pendingRewardCard  = null;
            waitingForDeckChoice = false;
            
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
            if (waitingForDeckChoice) return;

            if (selectedCard != null)
            {
                var picked = selectedCard.GetCurrentCard().DeepCopy();
                var deck = mainCharacter.deck;
                int maxCnt = (int)mainCharacter.GetStatValue(StatType.DeckSize);

                if (deck.cards.Count < maxCnt)
                {
                    deck.OnEvent(EventType.OnCardPurchase, picked);
                    base.NextStage();
                    return;
                }
                
                StartReplaceFlow(picked);
                return;
            }
            else if (goldSelected && goldView != null)
            {
                mainCharacter.gold += goldAmount;
                base.NextStage();
                return;
            }
        }

        void StartReplaceFlow(Card picked)
        {
            pendingRewardCard = picked;
            waitingForDeckChoice = true;
            
            deckView.Activate(
                mainCharacter.deck,
                requiredCount: 1,
                onConfirm: OnReplaceConfirmed,
                onCancel: OnReplaceCanceled,
                instructionText: "파괴할 카드 선택"
            );
            
            if(nextButton) nextButton.interactable = false;
        }

        void OnReplaceConfirmed(List<Card> cards)
        {
            var deck = mainCharacter.deck;
            var toRemove = cards[0];
            
            deck.OnEvent(EventType.OnCardRemove, toRemove);
            
            if(pendingRewardCard != null) deck.OnEvent(EventType.OnCardPurchase, pendingRewardCard);
            
            pendingRewardCard = null;
            waitingForDeckChoice = false;
            
            base.NextStage();
        }

        void OnReplaceCanceled()
        {
            pendingRewardCard = null;
            waitingForDeckChoice = false;
            if(nextButton) nextButton.interactable = (selectedCard != null) || goldSelected;
        }

        // 덱 버튼은 부모(OpenDeckInspectOnly)로 처리됨
    }
}
