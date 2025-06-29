using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;

namespace CardView
{
    public class CardView : MonoBehaviour
    {
        public Image illustrationImage;
        public Image expFill;
        public TMP_Text cardNameText;
        public TMP_Text levelText;
        public TMP_Text descriptionText;

        private Card card;

        public void SetCard(Card card)
        { 
            this.card = card;
            UpdateView();
        }

        public void UpdateView()
        {
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);

            cardNameText.text = card.cardName;
            descriptionText.text = card.cardDescription;
            levelText.text = $"Lv.{card.cardEnhancement.level.Value}";
        }
    }
}