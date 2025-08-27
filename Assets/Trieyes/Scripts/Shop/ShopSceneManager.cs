using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using CardViews;
using CharacterSystem;
using Stats;
using StickerSystem;
using Utils;
using GameFramework;
using UISystem;

public class ShopSceneManager : MonoBehaviour
{
    // ========= [싱글턴] =========
    public static ShopSceneManager Instance { get; private set; }

    // ========= [슬롯/뷰 프리팹 & 컨테이너] =========
    [Header("상점 슬롯 프리팹/컨테이너")]
    public GameObject shopCardSlot;
    public GameObject shopStickerSlot;
    public GameObject deckCardView;
    public GameObject shopScenePrefab;

    // ========= [상단/버튼/텍스트] =========
    [Header("버튼/텍스트 UI")]
    public Button sellButton;
    public Button rerollButton;
    public Button nextBattleButton;
    public TMP_Text sellPriceText;
    public TMP_Text rerollPriceText;
    public TMP_Text deckCountText;

    // ========= [플레이어/선택 상태] =========
    [HideInInspector] public Character mainCharacter;
    [HideInInspector] public CardView selectedCard1;
    [HideInInspector] public CardView selectedCard2;

    // ========= [정책 상수] =========
    private const int CARD_SELL_PRICE = 30;      // TODO: 레어별 가격 반영
    private const int INIT_REROLL_PRICE = 10;
    private const int CARD_PROB = 85;
    private const int STICKER_PROB = 15;
    private const int SLOT_COUNT = 4;

    private int rerollPrice;
    private Difficulty difficulty;

    // ========= [UI - 스탯/라운드/릴릭] =========
    [SerializeField] private TextMeshProUGUI textRoundInfo;
    [SerializeField] private TextMeshProUGUI textGold;
    [SerializeField] private List<Image> imageRelics;
    [SerializeField] private GameObject popupStatInfo;

    [Serializable]
    class StatTypeTMPPair
    {
        public StatType statType;
        public List<TextMeshProUGUI> text;
    }
    [SerializeField] private StatTypeTMPPair[] statTypeTMPPairs;

    // ========= [전체 레이아웃] =========
    [Header("전체 레이아웃")]
    [SerializeField] private RectTransform rectTransform;

    // ========= [덱/상점 스크롤 컨테이너] =========
    [Header("Deck Auto Scaling")]
    [SerializeField] private RectTransform DeckScaleRect;
    [SerializeField] private RectTransform ShopScaleRect;

    [SerializeField] private RectTransform DeckScaleRectParent;
    [SerializeField] private RectTransform ShopScaleRectParent;

    // ========= [전투 시작 애니메이션] =========
    [Header("전투 시작 애니메이션")]
    [SerializeField] private RectTransform rectOnBattleStartPopup;
    [SerializeField] private OnBattleStartPopupView onBattleStartPopupView;

    // ========= [영역별 비활성 패널] =========
    [Header("Inactive Panel")]
    [SerializeField] private Image stickerFlowPanel;

    // ========= [스티커 팝업] =========
    [SerializeField] private StickerApplyPopup stickerApplyPopup;

    // ========= [화면 사이즈 체크] =========
    private int lastScreenWidth;
    private int lastScreenHeight;

    // ========= [상태머신] =========
    public enum ShopMode { Normal, AwaitCardPick, StickerPopup }
    private ShopMode mode;
    public ShopMode CurrentMode => mode;

    // sticker attach 플로우용 pending 데이터(가격/결제는 슬롯 콜백으로 위임)
    private Sticker pendingSticker;
    private Func<bool> pendingCommit;             // 확정 시 결제/소모
    private Action pendingCancelReservation;      // 취소 시 예약 해제
    private CardView pendingTargetCardView;       // 확정 후 UI 갱신 대상

    // ========= [라이프사이클] =========
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        rectTransform.anchoredPosition = Vector2.zero;
        if (rectOnBattleStartPopup != null)
        {
            rectOnBattleStartPopup.gameObject.SetActive(false);
            rectOnBattleStartPopup.anchoredPosition = Vector2.zero;
        }
    }

    // ================= [초기화/비활성] =================
    public void Activate(Character mainCharacter, Difficulty difficulty)
    {
        Debug.Log("ShopSceneManager: Activate");
        this.mainCharacter = mainCharacter;
        this.difficulty = difficulty;

        shopScenePrefab.SetActive(true);

        rerollPrice = INIT_REROLL_PRICE;
        sellPriceText.text = CARD_SELL_PRICE.ToString();
        rerollPriceText.text = rerollPrice.ToString();

        UpdateDeckCountUI();
        UpdatePlayerRelics();
        OnScreenResized();
        RefreshShopSlots();
        SyncWithDeck();

        SetMode(ShopMode.Normal);
    }

    public void Deactivate()
    {
        shopScenePrefab.SetActive(false);
    }

    // ============= [프레임 동기화] =============
    private void Update()
    {
        if (mainCharacter == null || difficulty == null || !shopScenePrefab.activeSelf) return;
        UpdateRoundInfo();
        UpdatePlayerGold();
        UpdatePlayerStat();
    }

    private void LateUpdate()
    {
        CheckScreenResize();
    }

    // ============= [모드 전환/잠금] =============
    private void SetGlobalUIInteractable(bool enabled)
    {
        bool isExceed = mainCharacter != null && mainCharacter.deck.IsDeckExceed();

        rerollButton.interactable     = enabled && !isExceed;
        nextBattleButton.interactable = enabled && !isExceed;
        sellButton.interactable       = enabled && (selectedCard1 != null);
        // 필요 시: 상점 슬롯 버튼 일괄 On/Off 훅 추가
    }
    private void DeselectAllCards()
    {
        if (selectedCard1 != null) { selectedCard1.SetSelected(false); selectedCard1 = null; }
        if (selectedCard2 != null) { selectedCard2.SetSelected(false); selectedCard2 = null; }
        sellButton.interactable = false;
    }
    public void SetMode(ShopMode newMode)
    {
        mode = newMode;

        bool inactive = (mode != ShopMode.Normal);
        if (stickerFlowPanel) { stickerFlowPanel.gameObject.SetActive(inactive); }

        switch (mode)
        {
            case ShopMode.Normal:
                SetGlobalUIInteractable(true);
                stickerApplyPopup.Deactivate();
                break;
            case ShopMode.AwaitCardPick:
                DeselectAllCards();
                SetGlobalUIInteractable(false);
                stickerApplyPopup.Deactivate();
                break;
            case ShopMode.StickerPopup:
                DeselectAllCards();
                SetGlobalUIInteractable(false);
                break;
        }
    }

    // ============= [상점 UI 갱신] =============
    private void UpdateRoundInfo()
    {
        textRoundInfo.text = $"Stage {difficulty.stageNumber} - <color=#ff9>Shop</color> {1}";
    }

    private void UpdatePlayerGold()
    {
        textGold.text = $"{mainCharacter.gold}";
    }

    private void UpdatePlayerRelics()
    {
        for (int i = 0; i < mainCharacter.relics.Count; i++)
        {
            if (i >= imageRelics.Count)
                throw new Exception($"ShopSceneManager : Relic 아이콘 공간 부족.");
            var relic = mainCharacter.relics[i];
            var relicView = imageRelics[i];
            if (relic.icon is null)
                Debug.Log($"ShopSceneManager : Relic({relic.name})의 아이콘 없음.");
            relicView.sprite = relic.icon;
        }
    }

    private void UpdatePlayerStat()
    {
        foreach (var pair in statTypeTMPPairs)
        {
            var statValue = mainCharacter.statSheet[pair.statType].Value;
            foreach (var tmp in pair.text)
                tmp.text = statValue.ToString();
        }
    }

    // ============= [리사이즈 대응] =============
    private void CheckScreenResize()
    {
        if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height) return;

        OnScreenResized();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    private void OnScreenResized()
    {
        Canvas.ForceUpdateCanvases();
        AutoSizingOnScrollContent(DeckScaleRect, DeckScaleRectParent);
        AutoSizingOnScrollContent(ShopScaleRect, ShopScaleRectParent);
    }

    private void AutoSizingOnScrollContent(RectTransform target, RectTransform parentTransform)
    {
        if (target == null || parentTransform == null) return;

        float contentHeight = Vector2.Scale(target.rect.size, target.lossyScale).y;
        float parentHeight  = Vector2.Scale(parentTransform.rect.size, parentTransform.lossyScale).y;
        if (contentHeight <= 0f || parentHeight <= 0f) return;

        float k = parentHeight / contentHeight;
        var s = target.localScale;
        target.localScale = new Vector3(s.x * k, s.y * k, s.z);
    }

    // ============= [상점 슬롯 세팅] =============
    private void RefreshShopSlots()
    {
        foreach (Transform child in ShopScaleRect) Destroy(child.gameObject);

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            float rand = UnityEngine.Random.Range(0f, 100f);
            if (rand <= CARD_PROB)
                Instantiate(shopCardSlot, ShopScaleRect);
            else
                Instantiate(shopStickerSlot, ShopScaleRect);
        }
    }

    // ============= [덱 동기화] =============
    public void SyncWithDeck()
    {
        foreach (Transform child in DeckScaleRect) Destroy(child.gameObject);

        Deck deck = mainCharacter.deck;
        foreach (var card in deck.cards)
        {
            var obj = Instantiate(deckCardView, DeckScaleRect);
            obj.transform.localScale = Vector3.one;
            var cardView = obj.GetComponent<CardView>();

            cardView.SetCard(card);
            cardView.SetCanInteract(true);
        }
        UpdateButtonState();
        UpdateDeckCountUI();
    }

    private void UpdateButtonState()
    {
        bool isExceed = mainCharacter.deck.IsDeckExceed();
        rerollButton.interactable = !isExceed;
        nextBattleButton.interactable = !isExceed;
        deckCountText.color = isExceed ? Color.red : Color.white;
    }

    private void UpdateDeckCountUI()
    {
        deckCountText.text = $"Cards : {mainCharacter.deck.cards.Count} / {mainCharacter.deck.maxCardCount}";
    }

    // ============= [카드 클릭/병합/스왑/판매] =============
    public void OnCardClicked(CardView cardView)
    {
        switch (mode)
        {
            case ShopMode.Normal:
            {
                Deck deck = mainCharacter.deck;
                if (selectedCard1 == null)
                {
                    selectedCard1 = cardView;
                    selectedCard1.SetSelected(true);
                    sellButton.interactable = true;
                }
                else if (selectedCard1 == cardView)
                {
                    selectedCard1.SetSelected(false);
                    selectedCard1 = null;
                    sellButton.interactable = false;
                }
                else
                {
                    selectedCard2 = cardView;
                    selectedCard2.SetSelected(true);

                    var cardA = selectedCard1.GetCurrentCard();
                    var cardB = selectedCard2.GetCurrentCard();

                    if (cardA.cardName == cardB.cardName)
                        deck.MergeCards(cardA, cardB);
                    else
                        deck.SwapCards(cardA, cardB);

                    selectedCard1.SetSelected(false);
                    selectedCard2.SetSelected(false);
                    selectedCard1 = null;
                    selectedCard2 = null;
                    sellButton.interactable = false;
                    SyncWithDeck();
                }
                break;
            }

            case ShopMode.AwaitCardPick:
                pendingTargetCardView = cardView;
                OpenStickerPopup(cardView.GetCurrentCard());
                break;

            case ShopMode.StickerPopup:
                // 팝업 중에는 덱 카드 입력 무시
                break;
        }
    }

    // ============= [스티커 구매 플로우 입구] =============
    // 슬롯에서 가격/결제 책임을 갖고, 여기엔 콜백만 넘겨준다.
    public void BeginStickerAttachFlow(Sticker sticker, Func<bool> commitPurchase, Action cancelReservation)
    {
        if (mode != ShopMode.Normal) return;

        pendingSticker = sticker;
        pendingCommit = commitPurchase;
        pendingCancelReservation = cancelReservation;

        SetMode(ShopMode.AwaitCardPick);
    }

    // ============= [팝업 열기/확정/취소] =============
    private void OpenStickerPopup(Card targetCard)
    {
        SetMode(ShopMode.StickerPopup);

        // 미리보기 전용 복제본 (확정 전 원본 변경 금지)
        var preview = targetCard.DeepCopy();

        stickerApplyPopup.Activate(
            preview,
            pendingSticker,
            onConfirm: (paramIdx) => ConfirmStickerAttach(targetCard, preview, paramIdx),
            onCancel: CloseStickerAttachPopup
        );
    }

    private void CloseStickerAttachPopup()
    {
        // 덱 다시 고르는 화면으로 복귀
        SetMode(ShopMode.AwaitCardPick);
    }

    public void CancelStickerAttachFlow()
    {
        pendingCancelReservation?.Invoke();
        ClearPending();
        SetMode(ShopMode.Normal);
    }

    private void ClearPending()
    {
        pendingSticker = null;
        pendingCommit = null;
        pendingCancelReservation = null;
        pendingTargetCardView = null;
    }

    private void ConfirmStickerAttach(Card targetCard, Card previewCard, int paramIdx)
    {
        // 1) 결제/소모는 슬롯 콜백으로
        if (pendingCommit == null || !pendingCommit())
        {
            Debug.LogWarning("Purchase commit failed (not enough gold or slot invalid).");
            return; // 팝업 유지
        }

        // 2) 원본 카드에 최종 적용
        int charIndex = targetCard.paramCharRanges[paramIdx].start;
        bool ok = targetCard.TryApplyStickerOverrideAtCharIndex(charIndex, pendingSticker);
        if (!ok)
        {
            Debug.LogWarning("Sticker apply failed on original card.");
            // (정책에 따라 환불/롤백이 필요하면 commit을 validate/commit 두 단계로 분리)
            return;
        }

        // 3) 마무리
        if (stickerApplyPopup != null) stickerApplyPopup.Deactivate();
        pendingTargetCardView?.UpdateView();
        SyncWithDeck();

        ClearPending();
        SetMode(ShopMode.Normal);
    }

    // ============= [상점 기능 버튼] =============
    public void SellCard()
    {
        if (selectedCard1 == null) return;
        Deck deck = mainCharacter.deck;
        deck.RemoveCard(selectedCard1.GetCurrentCard());
        selectedCard1.SetSelected(false);
        selectedCard1 = null;
        sellButton.interactable = false;
        SyncWithDeck();
        mainCharacter.gold += CARD_SELL_PRICE;
    }

    public void Reroll()
    {
        if (mainCharacter.gold < rerollPrice)
        {
            Debug.LogError("Not enough gold to reroll");
            return;
        }
        mainCharacter.gold -= rerollPrice;
        rerollPrice += 10;
        rerollPriceText.text = rerollPrice.ToString();
        RefreshShopSlots();
    }

    public void ShowMeTheMoney()
    {
        mainCharacter.gold += 10000;
    }

    public void OnClickNextRound()
    {
        onBattleStartPopupView.Activate();

        CardStatChangeRecorder.Instance.RecordStart();
        mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
        var triggerResult = CardStatChangeRecorder.Instance.RecordEnd();

        onBattleStartPopupView.AnimateTriggerEvent(triggerResult);
    }

    public void OnClickStatInfo()
    {
        ToggleStatInfoPopup();
    }

    private void ToggleStatInfoPopup()
    {
        popupStatInfo.SetActive(!popupStatInfo.activeSelf);
    }

    public void StartNextBattleOnPopup()
    {
        UpdatePlayerStat();
        Deactivate();
        SceneChangeManager.Instance.ChangeShopToBattle((Character)mainCharacter);
    }
}
