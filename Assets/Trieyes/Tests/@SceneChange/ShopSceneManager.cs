using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using CardViews;
using DeckViews;
using CharacterSystem;
using BattleSystem;
using Stats;
using System;
using StickerSystem;
using System.Collections.Generic;
using GameFramework;
using Utils;
using System.Collections;

/// <summary>
/// 상점 씬의 핵심 관리 클래스입니다.
/// 카드 뽑기/구매/덱 갱신/전투 진입까지 UI-비즈니스 연결을 총괄.
/// </summary>
public class ShopSceneManager : MonoBehaviour
{
    [Header("카드 슬롯 UI")]
    public List<CardView> shopCardViews;    // 3개 카드뷰를 Inspector에서 순서대로 할당
    
    [Header("스티커 슬롯 UI")]
    public List<StickerView> shopStickerViews;

    [Header("카드 구매 버튼")]
    public List<Button> buyCardButtons;         // 3개 버튼을 Inspector에서 순서대로 할당
    
    [Header("스티커 구매 버튼")]
    public List<Button> buyStickerButtons;
    
    [Header("덱 UI 연동")]
    public DeckView deckView; 

    [Header("리롤 & 전투 진입 버튼")]
    public Button rerollButton;
    public Button battleButton;

    [Header("플레이어 및 스탯 UI")]
    public TMP_Text attackStatText;
    public TMP_Text defenseStatText;
    public TMP_Text healthStatText;
    public TMP_Text moveSpeedStatText;
    
    public static ShopSceneManager Instance;

    // --- 내부 필드 ---
    private List<Card> shopCards = new();
    private List<Sticker> shopStickers = new();
    
    public Sticker selectedSticker;
    private StickerView selectedStickerView;

    private Pawn mainCharacter;
    private Difficulty difficulty;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    // --- 초기화 ---
    public void Activate(Character mainCharacter, Difficulty difficulty)
    {
        this.mainCharacter = mainCharacter;
        this.difficulty = difficulty;
        
        mainCharacter.transform.SetParent(this.transform);
        // 2. 최초 상점 카드 리롤
        RefreshShopCards();

        // 3. 덱 UI 연동 (플레이어 덱 표시)
        deckView.SetDeck(mainCharacter.deck);

        // 4. 각 버튼 리스너 설정
        if (rerollButton != null) rerollButton.onClick.AddListener(RefreshShopCards);
        if (battleButton != null) battleButton.onClick.AddListener(OnBattleButtonPressed);

        for (int i = 0; i < buyCardButtons.Count; i++)
        {
            int idx = i; // capture for closure
            buyCardButtons[i].onClick.AddListener(() => OnCardBuyButtonPressed(idx));
        }

        for (int i = 0; i < buyStickerButtons.Count; i++)
        {
            int idx = i;
            buyStickerButtons[i].onClick.AddListener(() => OnStickerBuyButtonPressed(idx));
        }

        RefreshStatUI();
    }
    
    public void Deactivate()
    {
        // 1. 버튼 리스너 해제 (중복 AddListener 방지, 메모리 누수 방지)
        if (rerollButton != null) rerollButton.onClick.RemoveListener(RefreshShopCards);
        if (battleButton != null) battleButton.onClick.RemoveListener(OnBattleButtonPressed);

        for (int i = 0; i < buyCardButtons.Count; i++)
            buyCardButtons[i].onClick.RemoveAllListeners();

        for (int i = 0; i < buyStickerButtons.Count; i++)
            buyStickerButtons[i].onClick.RemoveAllListeners();

        // 2. 내부 참조 해제 (메모리 관리)
        //mainCharacter = null;
        selectedSticker = null;
        selectedStickerView = null;
        shopCards.Clear();
        shopStickers.Clear();

        // 3. UI 상태 초기화/리셋 (필요시)
        // 예: shopCardViews, shopStickerViews 초기화, 덱 UI 리셋 등

        // 4. 싱글턴 참조 해제 (씬 언로드 대비)
        Instance = null;

        // 5. 오브젝트 비활성화 (혹은 Destroy)
        // this.gameObject.SetActive(false); // 또는 Destroy(this.gameObject);
    }


    // --- 상점 카드 리롤 ---
    private void RefreshShopCards()
    {
        shopCards.Clear();
        shopStickers.Clear();
        for (int i = 0; i < shopCardViews.Count; i++)
        {
            Card newCard = CardFactory.Instance.Create(
                UnityEngine.Random.Range(1, 4),         // 카드 레벨(1~3)
                UnityEngine.Random.Range(0, CardFactory.Instance.cardInfos.Count)
            );
            shopCards.Add(newCard);
            shopCardViews[i].SetCard(newCard);
        }

        for (int i = 0; i < shopStickerViews.Count; i++)
        {
            Sticker newSticker = StickerFactory.Instance.CreateRandomSticker();
            shopStickers.Add(newSticker);
            shopStickerViews[i].SetSticker(newSticker);
        }
        // 구매 버튼 활성화 초기화
        for (int i = 0; i < buyCardButtons.Count; i++)
            buyCardButtons[i].interactable = true;
        for (int i = 0; i < buyStickerButtons.Count; i++)
            buyStickerButtons[i].interactable = true;
    }

    // --- 카드 구매 ---
    private void OnCardBuyButtonPressed(int index)
    {
        if (index < 0 || index >= shopCards.Count)
            return;

        var cardToBuy = shopCards[index];
        if (cardToBuy == null)
            return;

        // 덱에 딥카피로 추가
        mainCharacter.deck.AddCard(cardToBuy.DeepCopy());
        // UI 갱신
        deckView.RefreshDeckUI();
        buyCardButtons[index].interactable = false; // 중복구매 방지

        // 필요하다면 카드 구매 시마다 스탯도 갱신
        RefreshStatUI();
    }

    private void OnStickerBuyButtonPressed(int index)
    {
        if (index < 0 || index >= shopStickers.Count)
            return;

        // 1. 이전 선택 해제(SticekrView에서 selected 구현 되면 추가)
        // if (selectedStickerView != null)
        //     selectedStickerView.SetSelected(false);

        // 2. 새로 선택
        Debug.Log("<color=yellow>Sticker Selected!</color>");
        selectedSticker = shopStickers[index];
        selectedStickerView = shopStickerViews[index];

        // 3. 하이라이트 효과 (StickerView에서 구현 필요)
        //selectedStickerView.SetSelected(true);

        // 4. 카드뷰들에 "스티커 적용 모드" 안내 등 필요하다면 표시
        // (예시: CardView에 isStickerApplyMode 등)
    }
    
    private IEnumerator BattleButtonRoutine()
    {
        mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
        RefreshStatUI();

        yield return new WaitForSeconds(0.8f); // 0.8초 정도, 원하는 시간으로 변경

        this.Deactivate();
        TSceneChangeManager.Instance.ChangeShopToBattle((Character)mainCharacter);
    }

    // --- 전투 진입 ---
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

    // --- 스탯 UI 갱신 ---
    private void RefreshStatUI()
    {
        attackStatText.text = mainCharacter.statSheet[StatType.AttackPower].Value.ToString();
        defenseStatText.text = mainCharacter.statSheet[StatType.Defense].Value.ToString();
        healthStatText.text = mainCharacter.statSheet[StatType.Health].Value.ToString();
        moveSpeedStatText.text = mainCharacter.statSheet[StatType.MoveSpeed].Value.ToString();
    }
}