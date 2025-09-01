using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardViews;
using CardSystem;
using StickerSystem;

public class StickerApplyPopup : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private CardView CardView;       // 확대 카드뷰(프리뷰 전용)
    [SerializeField] private Button confirmButton;       // 확정
    [SerializeField] private Button backgroundButton;    // 배경(딤) 누르면 취소

    private Action<int> onConfirm;
    private Action onCancel;

    private Card previewCard;
    private Sticker sticker;
    private int selectedParamIdx = -1;

    public void Activate(Card preview, Sticker sticker, Action<int> onConfirm, Action onCancel)
    {
        gameObject.SetActive(true);

        this.previewCard = preview;
        this.sticker = sticker;
        this.onConfirm = onConfirm;
        this.onCancel = onCancel;
        this.selectedParamIdx = -1;

        // 프리뷰 카드 세팅
        CardView.SetCard(previewCard);
        CardView.SetCanInteract(true);

        // ▼ 디스크립션 탭을 "파라미터 선택" 모드로 전환
        CardView.EnableParamPickMode(sticker, (paramIdx) =>
        {
            selectedParamIdx = paramIdx;
            confirmButton.interactable = (selectedParamIdx >= 0);
        });

        // 초기는 확정 비활성
        confirmButton.interactable = false;
        
        OnResize();
    }

    public void Deactivate()
    {
        // 모드 원복
        CardView.DisableParamPickMode();

        // 상태 비움
        onConfirm = null;
        onCancel = null;
        previewCard = null;
        sticker = null;
        selectedParamIdx = -1;

        // 닫기
        gameObject.SetActive(false);
    }

    public void OnResize()
    {
        Canvas.ForceUpdateCanvases();
        RectTransform target = (RectTransform)transform;
        RectTransform view   = CardView.rectTransform;
        
        Vector2 viewWorldSize   = Vector2.Scale(view.rect.size,   view.lossyScale);
        Vector2 targetWorldSize = Vector2.Scale(target.rect.size, target.lossyScale);
        
        float desiredHeight = Mathf.Max(1f, targetWorldSize.y) * 0.6f;
        float scaleByHeight = desiredHeight / Mathf.Max(1f, viewWorldSize.y);
        
        view.localScale = new Vector3(
            view.localScale.x * scaleByHeight,
            view.localScale.y * scaleByHeight,
            1f
        );
    }

    private void Awake()
    {
        if (confirmButton)   confirmButton.onClick.AddListener(() =>
        {
            if (selectedParamIdx >= 0) onConfirm?.Invoke(selectedParamIdx);
        });
        if (backgroundButton) backgroundButton.onClick.AddListener(() => onCancel?.Invoke());
    }
}
