using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

namespace StickerSystem
{
    public class StickerView : MonoBehaviour
    {
        [Header("UI")] 
        public Image background;
        public TMP_Text valueText;
        public TMP_Text typeText;

        [Header("Backgrounds")]
        public Sprite addStickerBackground;
        public Sprite statTypeStickerBackground;
        public Sprite percentStickerBackground;

        private Sticker sticker;

        public void SetSticker(Sticker sticker)
        {
            this.sticker = sticker;
            UpdateView();
        }

        public Sticker GetSticker()
        {
            return sticker;
        }

        private void UpdateView()
        {
            if (sticker == null)
            {
                valueText.text = "";
                typeText.text = "";
                background.sprite = null;
                return;
            }

            if (sticker.type == StickerType.Add)
            {
                valueText.text = sticker.numberValue.ToString();
                typeText.text = "숫자 스티커";
                background.sprite = addStickerBackground;
            }
            else if (sticker.type == StickerType.StatType)
            {
                valueText.text = StatTypeTransformer.StatTypeToKorean(sticker.statTypeValue);
                typeText.text = "스탯 스티커";
                background.sprite = statTypeStickerBackground;
            }
            else if (sticker.type == StickerType.Percent)
            {
                valueText.text = sticker.numberValue.ToString();
                typeText.text = "% 스티커";
                background.sprite = percentStickerBackground;
            }
        }
    }
}