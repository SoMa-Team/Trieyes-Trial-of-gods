// CardEnhancementStage.cs
using CharacterSystem;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;
using CardViews;

namespace NodeStage
{
    public class CardEnhancementStage : MonoBehaviour, NodeStage
    {
        [SerializeField] private RectTransform rectTransform;

        [Header("UI")]
        [SerializeField] private Button btnEnhance;
        [SerializeField] private Button btnSwap;
        [SerializeField] private Button nextStageButton;

        [Header("Popup")]
        [SerializeField] private DeckView deckView;

        private Character mainCharacter;

        public static CardEnhancementStage Instance { get; private set; }

        private enum Mode { None, Enhance, Swap }
        private Mode currentMode;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
            
            btnEnhance?.onClick.AddListener(() => OpenDeckFor(Mode.Enhance));
            btnSwap?.onClick.AddListener(() => OpenDeckFor(Mode.Swap));
        }

        public void Activate(Character mainCharacter)
        {
            this.mainCharacter = mainCharacter;
            gameObject.SetActive(true);
            currentMode = Mode.None;
        }

        private void DeActivate()
        {
            gameObject.SetActive(false);
        }

        public void NextStage()
        {
            DeActivate();
            NextStageSelectPopup.Instance.SetNextStage(StageType.CardEnhancement, mainCharacter);
        }

        // ===== 내부 로직 =====
        private void OpenDeckFor(Mode mode)
        {
            if (mainCharacter == null || mainCharacter.deck == null) return;

            currentMode = mode;

            int need = (mode == Mode.Enhance) ? 1 : 2;

            deckView.Activate(
                mainCharacter.deck,
                need,
                onConfirm: OnDeckConfirm,
                onCancel: OnDeckCancel
            );
        }

        private void OnDeckCancel()
        {
            currentMode = Mode.None; // 아무 일도 하지 않고 종료
        }

        private void OnDeckConfirm(List<Card> picked)
        {
            if (picked == null || picked.Count == 0) return;

            switch (currentMode)
            {
                case Mode.Enhance:
                    ApplyEnhancement(picked[0]);
                    break;

                case Mode.Swap:
                    if (picked.Count >= 2)
                        mainCharacter.deck.SwapCards(picked[0], picked[1]);
                    break;
            }

            currentMode = Mode.None;
            NextStage();
        }

        // 강화 적용(프로젝트 규약에 맞춰 조정)
        private void ApplyEnhancement(Card card)
        {
            if (card == null || card.cardEnhancement == null) return;
            
            card.LevelUp();
        }
    }
}
