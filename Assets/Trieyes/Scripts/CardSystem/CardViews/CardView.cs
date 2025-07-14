using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using UnityEngine.EventSystems;
using DeckViews;

namespace CardViews
{
    /// <summary>
    /// 카드의 정보를 UI에 표시하고 선택/클릭 이벤트, 강조까지 지원하는 통합 뷰
    /// </summary>
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        // --- UI 필드 ---
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
        public Image selectionOutline; // (DeckCardView에서 합침!)

        // --- 내부 필드 ---
        private Card card;
        private DeckView parentDeckView;
        
        public void SetParentDeckView(DeckViews.DeckView deckView) => parentDeckView = deckView;

        // --- 카드 설정 및 UI ---
        public virtual void SetCard(Card card)
        {
            this.card = card;
            if (card.cardAction != null)
                card.cardAction.SetCard(card);
            SetSelected(false);
            UpdateView();
        }

        public Card GetCurrentCard()
        {
            if (card is null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return card;
        }

        public void UpdateView()
        {
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);
            Debug.Log($"<color=yellow>expFill.fillAmount: {expFill.fillAmount}</color>");

            cardNameText.text = card.cardName;
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

        // --- 선택 및 클릭 처리 (DeckCardView 기능 포함) ---

        public void OnPointerClick(PointerEventData eventData)
        {
            // 여기서 DeckZoneManager.Instance 혹은 다른 매니저로 전달
            parentDeckView?.OnCardClicked(this);
        }

        /// <summary>
        /// 카드가 선택되었는지 여부에 따라 강조 효과를 토글합니다.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
                selectionOutline.color = selected ? Color.yellow : Color.black;
        }
    }
}
