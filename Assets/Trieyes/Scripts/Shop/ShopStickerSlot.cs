using System.Collections;
using CardSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CharacterSystem;
using DeckViews;
using StickerSystem;
using Utils;

public class ShopStickerSlot : MonoBehaviour
{
    public Image backGround;
    public TMP_Text ValueText;
    public TMP_Text TitleText;
    public TMP_Text priceText;
    public GameObject disableOverlay;

    private Sticker sticker;
    
    private Color statTypeStickerColor = new Color(239/255f, 171/255f, 205/255f, 1f);
    private Color numberStickerColor = new Color(171 / 255f, 205 / 255f, 239 / 255f, 1f);

    private int price;

    private void Awake()
    {
        disableOverlay.SetActive(false);
        price = 50;
        priceText.text = price.ToString();
        SetSticker();
    }

    public void SetSticker()
    {
        sticker = StickerFactory.CreateRandomSticker();
        string value;
        switch (sticker.type)
        {
            case(StickerType.Number):
                value = sticker.numberValue.ToString();
                ValueText.text = value;
                TitleText.text = value + " 스티커";
                backGround.color = numberStickerColor;
                break;
            case(StickerType.StatType):
                value = StatTypeTransformer.StatTypeToKorean(sticker.statTypeValue);
                ValueText.text = value;
                TitleText.text = value + " 스티커";
                backGround.color = statTypeStickerColor;
                break;
            default:
                break;
        }
    }
    public Sticker GetCurrentSticker()
    {
        return sticker;
    }

    public void OnClickBuyButton()
    {
        Pawn mainCharacter = ShopSceneManager.Instance.mainCharacter;
        if (mainCharacter.gold < price)
        {
            Debug.LogError("Not enough gold");
            return;
        }
        mainCharacter.gold -= price;
        ShopSceneManager.Instance.selectedSticker = sticker;
    }
}