using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using CharacterSystem;
using StickerSystem;
using Utils;

/// <summary>
/// 상점에서 스티커를 관리하는 단일 슬롯 클래스.
/// UI 표시, 가격, 구매 처리 등 담당.
/// </summary>
public class ShopStickerSlot : MonoBehaviour
{
    [Header("UI Reference")]
    public Image backGround;
    public TMP_Text valueText;
    public TMP_Text titleText;
    public TMP_Text priceText;
    public GameObject disableOverlay;

    // ====== 가격 및 색상 상수 ======
    private static readonly int STICKER_PRICE = 50;
    [SerializeField] private readonly Color STAT_TYPE_STICKER_COLOR = new Color(239f / 255, 171f / 255, 205f / 255, 1f);
    [SerializeField] private readonly Color NUMBER_STICKER_COLOR    = new Color(171f / 255, 205f / 255, 239f / 255, 1f);
    [SerializeField] private readonly Color PROB_STICKER_COLOR    = new Color(171f / 255, 239f / 255, 198f / 255, 1f);

    private Sticker sticker;
    private bool isReserved = false;

    private void Awake()
    {
        disableOverlay.SetActive(false);
        priceText.text = STICKER_PRICE.ToString();
        SetSticker(); // 초기 랜덤 스티커 설정
    }

    /// <summary>
    /// 슬롯에 새로운 랜덤 스티커를 배정하고 UI 갱신
    /// </summary>
    public void SetSticker()
    {
        sticker = StickerFactory.CreateRandomSticker();
        isReserved = false;
        disableOverlay.SetActive(false);
        switch (sticker.type)
        {
            case StickerType.Number:
                valueText.text = sticker.numberValue.ToString();
                titleText.text = $"{sticker.numberValue} 스티커";
                backGround.color = NUMBER_STICKER_COLOR;
                break;

            case StickerType.StatType:
                var statKorean = StatTypeTransformer.StatTypeToKorean(sticker.statTypeValue);
                valueText.text = statKorean;
                titleText.text = $"{statKorean} 스티커";
                backGround.color = STAT_TYPE_STICKER_COLOR;
                break;
            
            case StickerType.Probability:
                valueText.text = $"{sticker.numberValue.ToString()} %";
                titleText.text = $"{sticker.numberValue}% 스티커";
                backGround.color = PROB_STICKER_COLOR;
                break;
            default:
                valueText.text = string.Empty;
                titleText.text = "알 수 없음";
                backGround.color = Color.gray;
                break;
        }
    }

    /// <summary>
    /// 현재 슬롯에 배정된 스티커 반환
    /// </summary>
    public Sticker GetCurrentSticker() => sticker;

    private void Reserve()
    {
        isReserved = true;
        disableOverlay.SetActive(true);
    }

    private void ReleaseReservation()
    {
        isReserved = false;
        disableOverlay.SetActive(false);
    }

    private bool TryCommitPurchase(Pawn buyer)
    {
        if (!isReserved) return false;
        if(buyer is null || buyer.gold < STICKER_PRICE) return false;
        buyer.gold-=STICKER_PRICE;
        isReserved = false;
        return true;
    }

    /// <summary>
    /// 스티커 구매 버튼 클릭 시 실행. 골드 차감 및 스티커 선택 반영
    /// </summary>
    public void OnClickBuyButton()
    {
        var shopManager = ShopSceneManager.Instance;
        Pawn mainCharacter = shopManager.mainCharacter;

        if (shopManager.CurrentMode != ShopSceneManager.ShopMode.Normal) return;

        if (mainCharacter.gold < STICKER_PRICE)
        {
            Debug.LogError("Not enough gold");
            return;
        }
        
        Reserve();

        shopManager.BeginStickerAttachFlow(
            sticker,
            commitPurchase: () => TryCommitPurchase(mainCharacter),
            cancelReservation: ReleaseReservation
        );
    }
}
