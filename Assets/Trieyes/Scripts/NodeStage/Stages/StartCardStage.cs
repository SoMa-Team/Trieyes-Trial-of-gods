using CharacterSystem;
using GameFramework;
using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using CardViews;
using UnityEngine.UI;

namespace NodeStage
{
    public class StartCardStage : MonoBehaviour, NodeStage
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform cardContainer;
        [SerializeField] private Button nextButton; 
        
        private Character mainCharacter;
        
        private readonly List<CardView> cardSlots = new();
        private CardView selected;
        
        public static StartCardStage Instance { get; private set; }

        private int defaultCardCount = 3;
        
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
        }
        
        public void Activate(Character mainCharacter)
        {
            this.mainCharacter = mainCharacter;
            this.gameObject.SetActive(true);
            SetupCardSlots();
        }

        private void DeActivate()
        {
            ClearSlots();
            this.gameObject.SetActive(false);
        }

        private void ClearSlots()
        {
            for (int i = 0; i < cardSlots.Count; i++)
            {
                if (cardSlots[i] != null)
                {
                    Destroy(cardSlots[i].gameObject);
                }
            }
            cardSlots.Clear();
            selected = null;
            if(nextButton != null) nextButton.interactable = false;
        }

        private void SetupCardSlots()
        {
            ClearSlots();
            for (int i = 0; i < defaultCardCount; i++)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.SetCard(CardFactory.Instance.RandomCreate());
                card.SetCanInteract(true);
                card.SetSelected(false);
                card.SetOnClicked(OnCardClicked);
                cardSlots.Add(card);
            }
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
            if (nextButton != null) nextButton.interactable = (selected != null);
        }
        public void NextStage()
        {
            if (selected != null)
            {
                var pickedCard = selected.GetCurrentCard().DeepCopy();
                mainCharacter.deck.AddCard(pickedCard);
            }
            DeActivate();
            NextStageSelectPopup.Instance.SetNextStage(StageType.StartCard, mainCharacter);
        }
    }
}