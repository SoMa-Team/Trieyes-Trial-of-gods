using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

namespace StickerSystem
{
    public class StickerView : MonoBehaviour
    {
        [Header("UI")] 
        public TMP_Text valueText;

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
                return;
            }

            if (sticker.type == StickerType.Number)
            {
                valueText.text = sticker.numberValue.ToString();
            }
            else if (sticker.type == StickerType.StatType)
            {
                valueText.text = StatTypeTransformer.StatTypeToKorean(sticker.statTypeValue);
            }
        }
    }
}