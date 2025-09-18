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
    public GameObject disableOverlay;

    // ====== 가격 및 색상 상수 ======
    [SerializeField] private readonly Color STAT_TYPE_STICKER_COLOR = new Color(239f / 255, 171f / 255, 205f / 255, 1f);
    [SerializeField] private readonly Color NUMBER_STICKER_COLOR    = new Color(171f / 255, 205f / 255, 239f / 255, 1f);
    [SerializeField] private readonly Color PROB_STICKER_COLOR    = new Color(171f / 255, 239f / 255, 198f / 255, 1f);

    private Sticker sticker;
    private bool isReserved = false;

    private void Awake()
    {
        disableOverlay.SetActive(false);
        SetRandomSticker(); // 초기 랜덤 스티커 설정
    }

    /// <summary>
    /// 슬롯에 새로운 랜덤 스티커를 배정하고 UI 갱신
    /// </summary>
    public void SetRandomSticker()
    {
        sticker = StickerFactory.CreateRandomSticker();
        isReserved = false;
        disableOverlay.SetActive(false);
        switch (sticker.type)
        {
            case StickerType.Add:
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
            
            case StickerType.Percent:
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
}
