// DeckView.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using CardSystem;

namespace CardViews
{
    public class DeckView : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;    
        [SerializeField] private Button overlayButton;   
        [SerializeField] private Button nextButton;      

        [SerializeField] private Transform cardContainer;  
        [SerializeField] private CardView cardPrefab;      

        private readonly List<CardView> spawned = new();
        private readonly HashSet<CardView> selected = new();

        private int requiredSelectCount;
        private Action<List<Card>> onConfirm;
        private Action onCancel;

        public void Activate(Deck deck, int requiredCount, Action<List<Card>> onConfirm, Action onCancel)
        {
            this.requiredSelectCount = requiredCount;
            this.onConfirm = onConfirm;
            this.onCancel = onCancel;

            gameObject.SetActive(true);
            if (panelRoot) panelRoot.SetActive(true);

            Build(deck);
            SetNextInteractable();
            HookButtons(true);
        }

        public void Deactivate()
        {
            HookButtons(false);
            Clear();
            if (panelRoot) panelRoot.SetActive(false);
            gameObject.SetActive(false);
        }

        private void HookButtons(bool on)
        {
            if (on)
            {
                if (overlayButton) overlayButton.onClick.AddListener(Cancel);
                if (nextButton) nextButton.onClick.AddListener(Confirm);
            }
            else
            {
                if (overlayButton) overlayButton.onClick.RemoveListener(Cancel);
                if (nextButton) nextButton.onClick.RemoveListener(Confirm);
            }
        }

        private void Build(Deck deck)
        {
            Clear();
            foreach (var card in deck.cards)
            {
                var cv = Instantiate(cardPrefab, cardContainer);
                cv.SetCard(card);
                cv.SetCanInteract(true);
                cv.SetSelected(false);
                cv.SetOnClicked(OnCardClicked);
                spawned.Add(cv);
            }
        }

        private void Clear()
        {
            foreach (var cv in spawned)
                if (cv) Destroy(cv.gameObject);
            spawned.Clear();
            selected.Clear();
        }

        private void OnCardClicked(CardView cv)
        {
            if (selected.Contains(cv))
            {
                selected.Remove(cv);
                cv.SetSelected(false);
            }
            else
            {
                // 선택 제한: requiredSelectCount
                if (selected.Count >= requiredSelectCount)
                {
                    // 가장 먼저 선택한 항목 하나 해제 후 새로 선택(UX 선택)
                    var first = selected.First();
                    first.SetSelected(false);
                    selected.Remove(first);
                }
                selected.Add(cv);
                cv.SetSelected(true);
            }
            SetNextInteractable();
        }

        private void SetNextInteractable()
        {
            if (nextButton) nextButton.interactable = (selected.Count == requiredSelectCount);
        }

        private void Confirm()
        {
            if (selected.Count != requiredSelectCount) return;
            var cards = selected.Select(s => s.GetCurrentCard()).ToList();
            
            Deactivate();
            onConfirm?.Invoke(cards);
        }

        private void Cancel()
        {
            onCancel?.Invoke();
            Deactivate();
        }
    }

}
