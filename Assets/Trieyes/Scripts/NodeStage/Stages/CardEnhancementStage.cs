using CharacterSystem;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CardSystem;
using CardViews;

namespace NodeStage
{
    public class CardEnhancementStage : EventStage<CardEnhancementStage>
    {
        [Header("UI")]
        [SerializeField] private Button btnEnhance;
        [SerializeField] private Button btnSwap;

        [Header("Popup")]
        [SerializeField] private DeckView deckViewLocal; // 부모의 deckView와 같다면 생략 가능

        private enum Mode { None, Enhance, Swap }
        private Mode currentMode = Mode.None;

        protected override void OnActivated()
        {
            btnEnhance?.onClick.RemoveAllListeners();
            btnSwap?.onClick.RemoveAllListeners();

            btnEnhance?.onClick.AddListener(() => OpenDeckFor(Mode.Enhance));
            btnSwap?.onClick.AddListener(() => OpenDeckFor(Mode.Swap));
        }

        private void OpenDeckFor(Mode mode)
        {
            if (mainCharacter == null || mainCharacter.deck == null) return;
            currentMode = mode;

            var dv = deckViewLocal != null ? deckViewLocal : deckView; // 우선순위
            if (dv == null) return;

            int need = (mode == Mode.Enhance) ? 1 : 2;
            dv.Activate(mainCharacter.deck, need, OnDeckConfirm, OnDeckCancel);
        }

        private void OnDeckCancel() => currentMode = Mode.None;

        private void OnDeckConfirm(List<Card> picked)
        {
            if (picked == null || picked.Count == 0) return;

            switch (currentMode)
            {
                case Mode.Enhance:
                    ApplyEnhancement(picked[0]);
                    break;
                case Mode.Swap:
                    if (picked.Count >= 2) mainCharacter.deck.SwapCards(picked[0], picked[1]);
                    break;
            }
            currentMode = Mode.None;
            base.NextStage(); // ✅ 즉시 다음 스테이지
        }

        private void ApplyEnhancement(Card card)
        {
            if (card == null) return;
            card.LevelUp(); // 프로젝트 규약에 맞는 강화 API
        }
    }
}
