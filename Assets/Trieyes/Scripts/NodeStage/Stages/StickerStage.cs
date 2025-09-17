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
        [SerializeField] private ShopStickerSlot stickerPrefab;
        [SerializeField] private Transform stickerContainer;
        [SerializeField] private StickerApplyPopup stickerApplyPopup;

        private Sticker pendingSticker;
        private readonly List<ShopStickerSlot> stickerSlots = new();
        
        private const int STICKER_COUNT = 3;

        protected override void OnActivated()
        {
            SetUpStickerSlots();
        }

        protected override void OnDeactivated()
        {
            ClearSlots();
        }

        private void ClearSlots()
        {
            foreach (var stickerSlot in stickerSlots)
            {
                Destroy(stickerSlot.gameObject);
            }
            stickerSlots.Clear();
            pendingSticker = null;
            
        }

        private void SetUpStickerSlots()
        {
            ClearSlots();

            for (int i = 0; i < STICKER_COUNT; i++)
            {
                var stickerSlot = Instantiate(stickerPrefab, stickerContainer);
                stickerSlot.SetRandomSticker();
                
                var btn = stickerSlot.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(() => OnStickerClicked(stickerSlot.GetCurrentSticker()));
                
                stickerSlots.Add(stickerSlot);
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