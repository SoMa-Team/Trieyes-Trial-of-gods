using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using CardSystem;
using CardViews;
using DeckViews;
using CharacterSystem;
using BattleSystem;
using Stats;
using StickerSystem;
using GameFramework;
using Utils;

/// <summary>
/// 상점 씬의 UI와 게임 로직을 연결하는 핵심 매니저 클래스.
/// - 카드/스티커 리롤, 구매, 덱 갱신, 전투 진입 등 상점에서의 모든 흐름을 관리한다.
/// </summary>
public class ShopSceneManager : MonoBehaviour
{
    // ========== [UI 연결] ==========
    [Header("카드/스티커 슬롯 UI")]
    public List<CardView> shopCardViews;        // 상점에 보여줄 카드뷰들 (Inspector에서 3개 할당)
    public List<StickerView> shopStickerViews;  // 상점에 보여줄 스티커뷰들

    [Header("구매 버튼")]
    public List<Button> buyCardButtons;         // 카드 구매 버튼 (3개)
    public List<Button> buyStickerButtons;      // 스티커 구매 버튼 (3개)

    [Header("덱 및 액션 버튼")]
    public DeckView deckView;                   // 플레이어의 덱 UI
    public Button rerollButton;                 // 리롤 버튼
    public Button battleButton;                 // 전투 시작 버튼
    public Button showMeTheMoneyButton;         // 돈복사 버튼(테스트용)

    [Header("플레이어 스탯 UI")]
    public TMP_Text attackStatText;
    public TMP_Text defenseStatText;
    public TMP_Text healthStatText;
    public TMP_Text moveSpeedStatText;
    public TMP_Text goldStatText;

    // ========== [상태 관리] ==========
    [HideInInspector] public Pawn mainCharacter;
    public static ShopSceneManager Instance { get; private set; }

    private List<Card> shopCards = new();
    private List<Sticker> shopStickers = new();
    private Sticker selectedSticker;
    private StickerView selectedStickerView;
    private Difficulty difficulty;

    // ======================== [생명주기] ========================
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 상점 씬 활성화 및 UI 초기화 (씬 진입 시 호출)
    /// </summary>
    public void Activate(Character mainCharacter, Difficulty difficulty)
    {
        this.mainCharacter = mainCharacter;
        this.difficulty = difficulty;

        // 부모 오브젝트 변경 (필요시)
        mainCharacter.transform.SetParent(this.transform);

        // 골드 보너스 (테스트용/개발용)
        mainCharacter.gold += 10;

        RefreshShopCards();         // 카드/스티커 리롤
        deckView.SetDeck(mainCharacter.deck); // 덱 UI 동기화
        HookButtonListeners();      // 버튼 리스너 연결
        RefreshStatUI();            // 스탯 UI 갱신
    }

    /// <summary>
    /// 상점 씬 비활성화 및 이벤트/참조 해제 (씬 종료/이동 시 호출)
    /// </summary>
    public void Deactivate()
    {
        UnhookButtonListeners();
        selectedSticker = null;
        selectedStickerView = null;
        shopCards.Clear();
        shopStickers.Clear();
        // Instance = null; // 싱글턴 해제는 필요시 활성화
    }

    // ========== [버튼 리스너 연결/해제] ==========
    private void HookButtonListeners()
    {
        rerollButton?.onClick.AddListener(RefreshShopCards);
        battleButton?.onClick.AddListener(OnBattleButtonPressed);
        showMeTheMoneyButton?.onClick.AddListener(OnShowMeTheMoneyButtonPressed);

        for (int i = 0; i < buyCardButtons.Count; i++)
        {
            int idx = i;
            buyCardButtons[i].onClick.AddListener(() => OnCardBuyButtonPressed(idx));
        }
        for (int i = 0; i < buyStickerButtons.Count; i++)
        {
            int idx = i;
            buyStickerButtons[i].onClick.AddListener(() => OnStickerBuyButtonPressed(idx));
        }
    }

    private void UnhookButtonListeners()
    {
        rerollButton?.onClick.RemoveListener(RefreshShopCards);
        battleButton?.onClick.RemoveListener(OnBattleButtonPressed);
        showMeTheMoneyButton?.onClick.RemoveListener(OnShowMeTheMoneyButtonPressed);

        foreach (var btn in buyCardButtons)
            btn.onClick.RemoveAllListeners();
        foreach (var btn in buyStickerButtons)
            btn.onClick.RemoveAllListeners();
    }

    // ======================= [상점 카드/스티커 리롤] =======================
    /// <summary>
    /// 상점에 노출될 카드/스티커를 새로 뽑아 UI를 갱신한다. (골드 차감)
    /// </summary>
    private void RefreshShopCards()
    {
        if (mainCharacter.gold < 10)
        {
            Debug.LogError("Not enough gold");
            return;
        }

        mainCharacter.gold -= 10;
        shopCards.Clear();
        shopStickers.Clear();

        for (int i = 0; i < shopCardViews.Count; i++)
        {
            var newCard = CardFactory.Instance.RandomCreate();
            shopCards.Add(newCard);
            shopCardViews[i].SetCard(newCard);
        }
        for (int i = 0; i < shopStickerViews.Count; i++)
        {
            var newSticker = StickerFactory.CreateRandomSticker();
            shopStickers.Add(newSticker);
            shopStickerViews[i].SetSticker(newSticker);
        }

        RefreshStatUI();
        SyncPurchaseButtons();
    }

    // ======================= [구매 및 UI 동기화] =======================
    /// <summary>
    /// 카드 구매 버튼 클릭 시 호출
    /// </summary>
    private void OnCardBuyButtonPressed(int index)
    {
        if (index < 0 || index >= shopCards.Count) return;
        if (mainCharacter.gold < 5)
        {
            Debug.LogError("Not enough gold");
            return;
        }

        mainCharacter.gold -= 5;
        var cardToBuy = shopCards[index];
        if (cardToBuy == null) return;

        mainCharacter.deck.AddCard(cardToBuy.DeepCopy()); // 덱에 추가(복사)
        deckView.RefreshDeckUI();                         // 덱 UI 갱신
        SyncPurchaseButtons();                            // 버튼 상태 갱신
        RefreshStatUI();                                  // 스탯 UI 갱신
    }

    /// <summary>
    /// 카드/스티커 구매 버튼 상태를 덱 상태에 맞춰 갱신
    /// </summary>
    public void SyncPurchaseButtons()
    {
        bool deckFull = mainCharacter.deck.IsDeckFull();
        foreach (var btn in buyCardButtons)
            btn.interactable = !deckFull;
    }

    /// <summary>
    /// 스티커 구매 버튼 클릭 시 호출
    /// </summary>
    private void OnStickerBuyButtonPressed(int index)
    {
        if (index < 0 || index >= shopStickers.Count) return;
        if (mainCharacter.gold < 5)
        {
            Debug.LogError("Not enough gold");
            return;
        }

        mainCharacter.gold -= 5;
        selectedSticker = shopStickers[index];
        selectedStickerView = shopStickerViews[index];
        // TODO: StickerView 하이라이트/모드 연동
        RefreshStatUI();
    }

    /// <summary>
    /// 테스트용: 골드 1만 지급
    /// </summary>
    private void OnShowMeTheMoneyButtonPressed()
    {
        mainCharacter.gold += 10000;
        RefreshStatUI();
    }

    // ======================= [전투 진입] =======================
    /// <summary>
    /// 전투 시작 버튼 클릭 시 호출 (비동기 루틴)
    /// </summary>
    private void OnBattleButtonPressed()
    {
        Debug.Log("BattleSceneChangeTest");
        if (mainCharacter == null)
        {
            Debug.LogError("캐릭터가 초기화되지 않았습니다.");
            return;
        }
        StartCoroutine(BattleButtonRoutine());
    }

    private IEnumerator BattleButtonRoutine()
    {
        mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
        RefreshStatUI();
        yield return new WaitForSeconds(0.8f); // 연출 대기(필요시 조정)
        this.Deactivate();
        SceneChangeManager.Instance.ChangeShopToBattle((Character)mainCharacter);
    }

    // ======================= [스탯 UI 갱신] =======================
    /// <summary>
    /// 플레이어의 스탯/골드 UI를 실시간으로 갱신한다.
    /// </summary>
    private void RefreshStatUI()
    {
        attackStatText.text     = mainCharacter.statSheet[StatType.AttackPower].Value.ToString();
        defenseStatText.text    = mainCharacter.statSheet[StatType.Defense].Value.ToString();
        healthStatText.text     = mainCharacter.statSheet[StatType.Health].Value.ToString();
        moveSpeedStatText.text  = mainCharacter.statSheet[StatType.MoveSpeed].Value.ToString();
        goldStatText.text       = mainCharacter.gold.ToString();
    }
}
