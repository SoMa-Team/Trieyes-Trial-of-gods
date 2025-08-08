using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Utils;

namespace StickerSystem
{
    public class StickerOverlayView : MonoBehaviour
    {
        [Header("UI")] 
        public TMP_Text valueText;

        public void UpdateText(string text, TMP_Text styleSource, Color? textColor = null)
        {
            valueText.text = text;
            if (styleSource == null) return;
            
            valueText.font = styleSource.font;
            valueText.fontSize = styleSource.fontSize;
            valueText.fontStyle = styleSource.fontStyle;
            valueText.fontWeight = styleSource.fontWeight;
            valueText.richText = styleSource.richText;
            valueText.characterSpacing = styleSource.characterSpacing;
            valueText.wordSpacing = styleSource.wordSpacing;
            valueText.color = textColor ?? styleSource.color;
            valueText.raycastTarget = false;
        }

    }
}