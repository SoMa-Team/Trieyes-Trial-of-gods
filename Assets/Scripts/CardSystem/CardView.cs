using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardSystem
{
    public class CardView : MonoBehaviour
    {
        [Header("UI Reference")]
        public Image illustrationImage;
        public TextMeshProUGUI statText;
        public TextMeshProUGUI descriptionText;
        public Image propertyIconOrBG;
        public TextMeshProUGUI levelExpText;

        public Card card;

        public void SetCard(Card card)
        {
            this.card = card;
            UpdateView();
        }

        public void UpdateView()
        {
            
        }
    }
}