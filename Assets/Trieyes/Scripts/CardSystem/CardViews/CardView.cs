using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using UnityEngine.EventSystems;
using DeckViews;
using System.Collections.Generic;

namespace CardViews
{
    /// <summary>
    /// 카드의 정보를 UI에 표시하고, 선택/클릭/강조/스티커 적용까지 처리하는 뷰 클래스.
    /// </summary>
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        // ===== [UI 필드] =====
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
        public Image selectionOutline;

        // ===== [내부 필드] =====
        private Card card;
        private DeckView parentDeckView;

        /// <summary>
        /// 상위 덱 뷰 참조 연결
        /// </summary>
        public void SetParentDeckView(DeckViews.DeckView deckView) => parentDeckView = deckView;

        /// <summary>
        /// 카드 정보 할당 및 UI 초기화
        /// </summary>
        public virtual void SetCard(Card card)
        {
            this.card = card;
            SetSelected(false);
            UpdateView();
        }

        /// <summary>
        /// 현재 할당된 카드 반환
        /// </summary>
        public Card GetCurrentCard()
        {
            if (card == null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return card;
        }

        /// <summary>
        /// 카드 정보에 따라 UI 갱신
        /// </summary>
        public void UpdateView()
        {
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);

            cardNameText.text = card.cardName;
            var descParams = card.GetEffectiveParamTexts();
            descriptionText.text = FormatDescription(card.cardDescription, descParams);
            levelText.text = $"Lv.{card.cardEnhancement.level.Value}";

            // --- 속성 엠블럼 처리 ---
            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = propertyEmblemImage.sprite != null;
            }
            else propertyEmblemImage.enabled = false;

            // --- 스탯 엠블럼 및 값 표시 ---
            if (card.cardStats.stats.Count > 0 && statTypeEmblemTable != null)
            {
                var stat = card.cardStats.stats[0];
                statTypeEmblemImage.sprite = statTypeEmblemTable.GetEmblem(stat.type);
                statTypeEmblemImage.enabled = statTypeEmblemImage.sprite != null;
                statIntegerValueText.text = $"+{stat.value.Value}";
                statIntegerValueText.enabled = true;
            }
            else
            {
                statTypeEmblemImage.enabled = false;
                statIntegerValueText.enabled = false;
            }
        }

        /// <summary>
        /// 카드 설명(템플릿)에 실제 파라미터 값을 대입해 반환
        /// </summary>
        private string FormatDescription(string template, List<string> descParams)
        {
            if (descParams == null || descParams.Count == 0)
                return template;

            string result = template;
            for (int i = 0; i < descParams.Count; i++)
                result = result.Replace("{" + i + "}", descParams[i]);
            return result;
        }

        /// <summary>
        /// 카드 설명 클릭 시 단어 인덱스 계산 & 스티커 적용 시도.  
        /// 아니면 부모 덱 뷰로 카드 클릭 알림.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 설명 영역 클릭 시만 처리
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    descriptionText.rectTransform, eventData.position, eventData.pressEventCamera))
            {
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(
                    descriptionText, eventData.position, eventData.pressEventCamera);
                
                Debug.Log($"[CardView] 클릭 단어 인덱스: {wordIndex}");
            
                if (wordIndex != -1)
                {
                    var sticker = ShopSceneManager.Instance?.selectedSticker;
                    if (sticker != null)
                    {
                        bool applied = card.TryApplyStickerOverride(wordIndex, sticker);
                        if (applied)
                        {
                            UpdateView();
                            Debug.Log($"[CardView] 스티커가 {wordIndex}번째 파라미터에 적용됨");
                        }
                        else
                        {
                            Debug.LogWarning("[CardView] 스티커 적용 실패: 타입 불일치 또는 불가");
                            // (추후 피드백 UI 가능)
                        }
                        return;
                    }
                    // 스티커 없으면 아래로 Fall-through
                }
            }
            parentDeckView?.OnCardClicked(this); // 카드 선택 등 기존 처리
        }

        /// <summary>
        /// 카드가 선택 상태면 강조, 아니면 기본 색
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
                selectionOutline.color = selected ? Color.yellow : Color.black;
        }
    }
}
