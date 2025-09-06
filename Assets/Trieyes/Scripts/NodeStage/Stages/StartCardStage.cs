using CharacterSystem;
using GameFramework;
using UnityEngine;
using System.Collections.Generic;
using CardSystem;
using CardViews;

namespace NodeStage
{
    public class StartCardStage : MonoBehaviour, NodeStage
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private CardView cardPrefab;
        [SerializeField] private Transform cardContainer;
        private Character mainCharacter;
        
        private readonly List<CardView> cardSlots = new();
        
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
        }

        private void SetupCardSlots()
        {
            ClearSlots();
            for (int i = 0; i < defaultCardCount; i++)
            {
                var card = Instantiate(cardPrefab, cardContainer);
                card.SetCard(CardFactory.Instance.RandomCreate());
            }
        }

        public void NextStage()
        {
            DeActivate();
            NextStageSelectPopup.Instance.SetNextStage(StageType.StartCard, mainCharacter);
        }
    }
}