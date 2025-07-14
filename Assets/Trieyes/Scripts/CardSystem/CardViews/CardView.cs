using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;

namespace CardViews
{
    /// <summary>
    /// 카드의 정보를 UI에 표시하는 컴포넌트입니다.
    /// 카드의 일러스트, 이름, 레벨, 설명, 속성, 스탯 등을 갱신합니다.
    /// </summary>
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

        public virtual void SetCard(Card card)
        {
            this.card = card;
            // 액션에 카드 연결 보장 (생성시점에 이미 연결되어 있다면 아래는 필요 X)
            if (card.cardAction != null)
                card.cardAction.SetCard(card);
            UpdateView();
        }

        public Card GetCurrentCard()
        {
            if(this.card is null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return this.card;
        }

        public void UpdateView()
        {
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);

            cardNameText.text = card.cardName;
            // 변경: cardAction.GetDescriptionParams()만 호출
            var descParams = card.cardAction.GetDescriptionParams();
            descriptionText.text = FormatDescription(card.cardDescription, descParams);
            levelText.text = $"Lv.{card.cardEnhancement.level.Value}";
            
            // 속성 엠블럼 표시
            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = (propertyEmblemImage.sprite != null);
            }
            else
            {
                propertyEmblemImage.enabled = false;
            }
            
            // 스탯 엠블럼 및 값 표시
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
        
        private string FormatDescription(string template, string[] descParams)
        {
            if (descParams == null || descParams.Length == 0)
                return template;

            string result = template;
            for (int i = 0; i < descParams.Length; i++)
                result = result.Replace("{" + i + "}", descParams[i]);
            return result;
        }
    }
}
