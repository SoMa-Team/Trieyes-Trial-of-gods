using System;
using UnityEngine;
using TMPro;
using CardSystem;
using CardViews;
using CharacterSystem;
using DeckViews;

/// <summary>
/// 상점에 노출되는 단일 카드 슬롯(구매, UI, 가격 관리)을 담당
/// </summary>
public class ShopCardSlot : MonoBehaviour
{
    [Header("UI Reference")]
    public CardView cardView;            // 카드 정보 표시용
    public TMP_Text priceText;           // 가격 텍스트
    public GameObject disableOverlay;    // 구매 불가 시 오버레이

    // 각 레어리티별 가격 (상수, 필요시 config로)
    private static readonly int COMMON_PRICE    = 40;
    private static readonly int UNCOMMON_PRICE  = 60;
    private static readonly int LEGENDARY_PRICE = 70;
    private static readonly int EXCEED_PRICE    = 80;

    private int price = 0;

    private void Awake()
    {
        // 최초 진입 시 슬롯 비활성화 해제 및 랜덤 카드 배정
        disableOverlay.SetActive(false);
        SetCard(CardFactory.Instance.RandomCreate());
    }

    /// <summary>
    /// 슬롯에 카드를 배정하고 가격/UI 갱신
    /// </summary>
    public void SetCard(Card card)
    {
        cardView.SetCard(card);

        price = GetPriceByRarity(card.rarity);
        priceText.text = price.ToString();
    }

    /// <summary>
    /// 레어리티별 가격 반환 (추후 가격 정책 바뀌면 여기서만 변경)
    /// </summary>
    private int GetPriceByRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common    => COMMON_PRICE,
            Rarity.Uncommon  => UNCOMMON_PRICE,
            Rarity.Legendary => LEGENDARY_PRICE,
            Rarity.Exceed    => EXCEED_PRICE,
            _                => COMMON_PRICE
        };
    }

    /// <summary>
    /// 현재 슬롯에 할당된 카드 반환
    /// </summary>
    public Card GetCurrentCard() => cardView.GetCurrentCard();

    /// <summary>
    /// 카드 구매 버튼 클릭 시 실행. 골드 차감/덱 추가/UI 갱신 처리
    /// </summary>
    public void OnClickBuyButton()
    {
        var shopManager = NewShopSceneManager.Instance;
        Pawn mainCharacter = shopManager.mainCharacter;

        if (mainCharacter.gold < price)
        {
            Debug.LogError("Not enough gold");
            return;
        }

        // 결제 및 덱 추가
        mainCharacter.gold -= price;
        mainCharacter.deck.AddCard(GetCurrentCard().DeepCopy());

        // 슬롯 비활성화 (구매 완료 표시)
        disableOverlay.SetActive(true);

        // 덱 UI 동기화
        shopManager.SyncWithDeck();

        // 덱 한도 초과 시 버튼/텍스트 비활성화 처리
        if (mainCharacter.deck.IsDeckExceed())
        {
            shopManager.rerollButton.interactable = false;
            shopManager.nextBattleButton.interactable = false;
            shopManager.deckCountText.color = Color.red;
        }
    }
}
