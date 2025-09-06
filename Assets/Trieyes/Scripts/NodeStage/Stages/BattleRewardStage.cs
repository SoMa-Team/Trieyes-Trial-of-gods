using CharacterSystem;
using GameFramework;
using UnityEngine;
using CardViews;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;

namespace NodeStage
{
    public class BattleRewardStage : MonoBehaviour, NodeStage
    {
        [SerializeField] private RectTransform rectTransform;
        private Character mainCharacter;
        
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform rewardContainer;
        [SerializeField] private GoldRewardView goldRewardPrefab;
        
        [SerializeField] private Button nextButton;
        
        [SerializeField] private int cardChoiceCount = 3;
        [SerializeField] private int goldAmount = 20;
        
        public static BattleRewardStage Instance { get; private set; }
        
        private readonly List<CardView> cardSlots = new();
        private GoldRewardView goldView;

        private CardView selectedCard;
        private bool goldSelected;
        
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
            if (nextButton) nextButton.interactable = false;
        }
        
        public void Activate(Character mainCharacter)
        {
            this.mainCharacter = mainCharacter;
            this.gameObject.SetActive(true);
        }

        private void DeActivate()
        {
            ClearRewards();
            this.gameObject.SetActive(false);
        }
        private void ClearRewards()
        {
            foreach (var cv in cardSlots)
                if (cv != null) Destroy(cv.gameObject);
            cardSlots.Clear();

            if (goldView != null) { Destroy(goldView.gameObject); goldView = null; }

            selectedCard = null;
            goldSelected = false;
            if (nextButton) nextButton.interactable = false;
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

        public void NextStage()
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

            DeActivate();
            NextStageSelectPopup.Instance.SetNextStage(StageType.BattleReward, mainCharacter);
        }
    }
}