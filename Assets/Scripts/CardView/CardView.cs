using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using UnityEngine.Serialization;

namespace CardView
{
    public class CardView : MonoBehaviour
    {
        public Image illustrationImage;
        public Image expFill;
        public TMP_Text cardNameText;
        public TMP_Text levelText;
        public TMP_Text descriptionText;
        
        public Image propertyEmblemImage;
        public PropertyEmblemSO propertyEmblemTable;
        
        public StatTypeEmblemSO statTypeEmblemTable;
        public Image statTypeEmblemImage;
        public TMP_Text statIntegerValueText;

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
            
            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = (propertyEmblemImage.sprite != null); // 없으면 비활성화
            }
            else
            {
                propertyEmblemImage.enabled = false;
            }
            
            if (card.cardStats.stats.Count > 0 && statTypeEmblemTable != null)
            {
                var stat = card.cardStats.stats[0];
                statTypeEmblemImage.sprite = statTypeEmblemTable.GetEmblem(stat.type);
                statTypeEmblemImage.enabled = (statTypeEmblemImage.sprite != null);
                statIntegerValueText.text = $"+{stat.value.Value}";
                statIntegerValueText.enabled = true;
            }
            else
            {
                statTypeEmblemImage.enabled = false;
                statIntegerValueText.enabled = false;
            }
        }
    }
}