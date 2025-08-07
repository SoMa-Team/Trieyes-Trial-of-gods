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

/// <summary>
/// 상점(Shop) 씬의 핵심 관리 매니저.  
/// 카드/스티커 뽑기, 덱 관리, 골드/스탯/릴릭 UI, 슬롯 생성, 버튼 동작 등 통합 제어.
/// </summary>
public class NewShopSceneManager : MonoBehaviour
{
    // ========= [싱글턴 및 주요 필드] =========
    public static NewShopSceneManager Instance { get; private set; }

    [Header("상점 슬롯 프리팹/컨테이너")]
    public GameObject shopCardSlot;
    public GameObject shopStickerSlot;
    public GameObject deckCardView;

    [Header("버튼/텍스트 UI")]
    public Button sellButton;
    public Button rerollButton;
    public Button nextBattleButton;
    public TMP_Text sellPriceText;
    public TMP_Text rerollPriceText;
    public TMP_Text deckCountText;

    [Header("플레이어/카드 선택 상태")]
    [HideInInspector] public Character mainCharacter;
    [HideInInspector] public CardView selectedCard1;
    [HideInInspector] public CardView selectedCard2;
    [HideInInspector] public Sticker selectedSticker;

    // ======= [내부 정책 상수] =======
    private const int CARD_SELL_PRICE = 30;         // TODO: 레어별 가격 반영
    private const int INIT_REROLL_PRICE = 10;
    private const int CARD_PROB = 85;               // 카드 등장 확률(%)
    private const int STICKER_PROB = 15;
    private const int SLOT_COUNT = 4;

    private int rerollPrice;
    private Difficulty difficulty;

    // ====== [UI - 스탯/라운드/릴릭] ======
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

    // ====== [UI - 카드/상점 슬롯 컨테이너] ======
    [SerializeField] private RectTransform DeckScaleRect;
    [SerializeField] private RectTransform ShopScaleRect;
    
    [SerializeField] private RectTransform DeckScaleRectParent;
    [SerializeField] private RectTransform ShopScaleRectParent;

    // ====== [화면 사이즈 체크] ======
    private int lastScreenWidth;
    private int lastScreenHeight;

    // ====== [라이프사이클] ======
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ================= [초기화 및 비활성화] =================
    public void Activate(Character mainCharacter, Difficulty difficulty)
    {
        this.mainCharacter = mainCharacter;
        this.difficulty = difficulty;

        rerollPrice = INIT_REROLL_PRICE;
        sellPriceText.text = CARD_SELL_PRICE.ToString();
        rerollPriceText.text = rerollPrice.ToString();
        UpdateDeckCountUI();
        UpdatePlayerRelics();
        OnScreenResized();
        RefreshShopSlots();
        SyncWithDeck();
    }

    public void Deactivate()
    {
        // 필요시 리스너 해제, 상태/참조 정리 등 추가
    }

    // ============= [매 프레임 UI 상태 동기화] =============
    private void Update()
    {
        UpdateRoundInfo();
        UpdatePlayerGold();
        UpdatePlayerStat();
    }

    private void LateUpdate()
    {
        CheckScreenResize();
    }

    // ============= [상점 UI/상태 갱신 함수] =============
    private void UpdateRoundInfo()
    {
        // TODO: Stage, Round 구분 도입 필요
        textRoundInfo.text = $"Stage {difficulty.stageNumber} - <color=#ff9>Shop</color> {1}";
    }

    private void UpdatePlayerGold()
    {
        // TODO: 골드 3자리마다 콤마 등 서식 적용 가능
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

    // ============= [화면 사이즈 동기화] =============
    private void CheckScreenResize()
    {
        if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            return;
        
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

    private void AutoSizingOnScrollContent(RectTransform transform, RectTransform parentTransform)
    {
        var height = Vector2.Scale(transform.rect.size, transform.lossyScale).y;
        var parentHeight = Vector2.Scale(parentTransform.rect.size, parentTransform.lossyScale).y;
        transform.localScale *= parentHeight / height * Vector2.one;
    }

    // ============= [상점 슬롯 및 덱 동기화] =============
    private void RefreshShopSlots()
    {
        foreach (Transform child in ShopScaleRect)
            Destroy(child.gameObject);

        for (int i = 0; i < SLOT_COUNT; i++)
        {
            float rand = UnityEngine.Random.Range(0f, 100f);
            if (rand <= CARD_PROB)
                Instantiate(shopCardSlot, ShopScaleRect);
            else
                Instantiate(shopStickerSlot, ShopScaleRect);
        }
    }

    public void SyncWithDeck()
    {
        foreach (Transform child in DeckScaleRect)
            Destroy(child.gameObject);

        Deck deck = mainCharacter.deck;
        foreach (var card in deck.cards)
        {
            var obj = Instantiate(deckCardView, DeckScaleRect);
            obj.transform.localScale = Vector3.one;
            var cardView = obj.GetComponent<CardView>();
            cardView.SetCard(card);
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

    // ============= [카드 클릭/선택/병합/스왑/판매] =============
    public void OnCardClicked(CardView cardView)
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
    }

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

    // ============= [상점 기능 버튼] =============
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
        mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
        UpdatePlayerStat();
    }

    public void OnClickStatInfo()
    {
        ToggleStatInfoPopup();
    }

    private void ToggleStatInfoPopup()
    {
        popupStatInfo.SetActive(!popupStatInfo.activeSelf);
    }
}
