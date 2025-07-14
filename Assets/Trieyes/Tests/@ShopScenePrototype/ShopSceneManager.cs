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
using System.Collections.Generic;

/// <summary>
/// 상점 씬의 핵심 관리 클래스입니다.
/// 카드 뽑기/구매/덱 갱신/전투 진입까지 UI-비즈니스 연결을 총괄.
/// </summary>
public class ShopSceneManager : MonoBehaviour
{
    [Header("카드 슬롯 UI")]
    public List<CardView> shopCardViews;    // 3개 카드뷰를 Inspector에서 순서대로 할당

    [Header("구매 버튼")]
    public List<Button> buyButtons;         // 3개 버튼을 Inspector에서 순서대로 할당
    
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

    // --- 내부 필드 ---
    private Pawn mainCharacter;
    private List<Card> shopCards = new();

    // --- 초기화 ---
    private void Start()
    {
        // 1. 플레이어/캐릭터 초기화
        mainCharacter = CharacterFactory.Instance.Create(0);

        // 2. 최초 상점 카드 리롤
        RefreshShopCards();

        // 3. 덱 UI 연동 (플레이어 덱 표시)
        deckView.SetDeck(mainCharacter.deck);

        // 4. 각 버튼 리스너 설정
        if (rerollButton != null) rerollButton.onClick.AddListener(RefreshShopCards);
        if (battleButton != null) battleButton.onClick.AddListener(OnBattleButtonPressed);

        for (int i = 0; i < buyButtons.Count; i++)
        {
            int idx = i; // capture for closure
            buyButtons[i].onClick.AddListener(() => OnBuyButtonPressed(idx));
        }

        RefreshStatUI();
    }

    // --- 상점 카드 리롤 ---
    private void RefreshShopCards()
    {
        shopCards.Clear();
        for (int i = 0; i < shopCardViews.Count; i++)
        {
            Card newCard = CardFactory.Instance.Create(
                UnityEngine.Random.Range(1, 4),         // 카드 레벨(1~3)
                UnityEngine.Random.Range(0, CardFactory.Instance.cardInfos.Count)
            );
            shopCards.Add(newCard);
            shopCardViews[i].SetCard(newCard);
        }
        // 구매 버튼 활성화 초기화
        for (int i = 0; i < buyButtons.Count; i++)
            buyButtons[i].interactable = true;
    }

    // --- 카드 구매 ---
    private void OnBuyButtonPressed(int index)
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
        buyButtons[index].interactable = false; // 중복구매 방지

        // 필요하다면 카드 구매 시마다 스탯도 갱신
        RefreshStatUI();
    }

    // --- 전투 진입 ---
    private void OnBattleButtonPressed()
    {
        Debug.Log("BattleSceneChangeTest");
        if(mainCharacter == null)
        {
            Debug.LogError("캐릭터가 초기화되지 않았습니다.");
            return;
        }
        mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
        RefreshStatUI();
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