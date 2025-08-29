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
        public TMP_Text lifeText;

        [Header("Backgrounds")]
        public Sprite numberStickerBackground;
        public Sprite statTypeStickerBackground;

        private Sticker sticker;

        public void SetSticker(Sticker sticker)
        {
            this.sticker = sticker;
            UpdateView();
        }

        private void UpdateView()
        {
            if (sticker == null)
            {
                valueText.text = "";
                typeText.text = "";
                lifeText.text = "";
                background.sprite = null;
                return;
            }

            if (sticker.type == StickerType.Number)
            {
                valueText.text = sticker.numberValue.ToString();
                typeText.text = "숫자 스티커";
                background.sprite = numberStickerBackground;
            }
            else if (sticker.type == StickerType.StatType)
            {
                valueText.text = StatTypeTransformer.StatTypeToKorean(sticker.statTypeValue);
                typeText.text = "스탯 스티커";
                background.sprite = statTypeStickerBackground;
            }

            lifeText.text = $"남은 수명 : {sticker.lifeTime}";
        }
    }
}