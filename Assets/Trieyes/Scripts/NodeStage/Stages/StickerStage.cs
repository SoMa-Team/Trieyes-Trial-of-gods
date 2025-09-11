using UnityEngine;
using UnityEngine.UI;
using StickerSystem;
using CardViews;
using System.Collections.Generic;
using Stats;
using System;
using CardSystem;

namespace NodeStage
{
    public class StickerStage : EventStage<StickerStage>
    {
        [SerializeField] private StickerView stickerPrefab;
        [SerializeField] private Transform stickerContainer;
        
        [SerializeField] private DeckView deckView;
        [SerializeField] private StickerApplyPopup stickerApplyPopup;

        private Sticker pendingSticker;
        private readonly List<StickerView> stickerViews = new();

        protected override void OnActivated()
        {
            SetUpStickerSlots();
        }

        protected override void OnDeactivated()
        {
            ClearSlots();
        }

        private List<Sticker> CreateStickerPool()
        {
            var pool = new List<Sticker>();
            
            pool.Add(StickerFactory.CreateNumberSticker(UnityEngine.Random.Range(1, 100)));
            pool.Add(StickerFactory.CreateProbabilitySticker(UnityEngine.Random.Range(1, 40)));
            var stats = (StatType[])Enum.GetValues(typeof(StatType));
            var stat = stats[UnityEngine.Random.Range(0, stats.Length)];
            pool.Add(StickerFactory.CreateStatTypeSticker(stat));

            return pool;
        }

        private void ClearSlots()
        {
            foreach (var stickerView in stickerViews)
            {
                Destroy(stickerView.gameObject);
            }
            stickerViews.Clear();
            pendingSticker = null;
            
        }

        private void SetUpStickerSlots()
        {
            ClearSlots();
            
            var pool = CreateStickerPool();
            for (int i = 0; i < pool.Count; i++)
            {
                Sticker sticker = pool[i];
                var stickerView = Instantiate(stickerPrefab, stickerContainer);
                stickerView.SetSticker(sticker);
                
                var btn = stickerView.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => OnStickerClicked(stickerView.GetSticker()));
                
                stickerViews.Add(stickerView);
            }
        }

        private void OnStickerClicked(Sticker sticker)
        {
            pendingSticker = sticker;
            deckView.Activate(
                mainCharacter.deck,
                requiredCount: 1,
                onConfirm: OnCardPicked,
                onCancel: OnCardPickCanceled
            );
        }
        
        private void OnCardPicked(List<Card> cards)
        {
            var target = cards[0];
            OpenStickerPopup(target);
        }

        private void OnCardPickCanceled()
        {
            pendingSticker = null;
        }
        
        private void OpenStickerPopup(Card targetCard)
        {
            var preview = targetCard.DeepCopy();

            stickerApplyPopup.Activate(
                preview,
                pendingSticker,
                onConfirm: (paramIdx) => ConfirmStickerAttach(targetCard, preview, paramIdx),
                onCancel: CloseStickerPopup
            );
        }
        
        private void ConfirmStickerAttach(Card targetCard, Card previewCard, int paramIdx)
        {
            targetCard.RemoveStickerOverridesByInstance(pendingSticker);
            bool ok = targetCard.TryApplyStickerOverrideAtParamIndex(paramIdx, pendingSticker);
            if (!ok)
            {
                Debug.LogWarning("StickerStage: 스티커 적용 실패(해당 슬롯 불가 등).");
                return;
            }

            stickerApplyPopup.Deactivate();
            CloseStickerPopup();
            NextStage();
        }

        private void CloseStickerPopup()
        {
            pendingSticker = null;
        }
    }
}