using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

namespace CardViews
{
    /// <summary>
    /// 카드의 정보를 UI에 표시하는 컴포넌트입니다.
    /// 카드의 일러스트, 이름, 레벨, 설명, 속성, 스탯 등을 갱신합니다.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        // --- UI 필드 ---

        /// 카드 일러스트 이미지
        public Image illustrationImage;
        /// 경험치 게이지 이미지
        public Image expFill;
        /// 카드 이름 텍스트
        public TMP_Text cardNameText;
        /// 카드 레벨 텍스트
        public TMP_Text levelText;
        /// 카드 설명 텍스트
        public TMP_Text descriptionText;
        /// 속성 엠블럼 이미지
        public Image propertyEmblemImage;
        /// 속성 엠블럼 테이블 (ScriptableObject)
        public PropertyEmblemSO propertyEmblemTable;
        /// 스탯 타입 엠블럼 테이블 (ScriptableObject)
        public StatTypeEmblemSO statTypeEmblemTable;
        /// 스탯 타입 엠블럼 이미지
        public Image statTypeEmblemImage;
        /// 스탯 값 텍스트
        public TMP_Text statIntegerValueText;

        // --- 내부 필드 ---

        /// 현재 표시 중인 카드 데이터
        public Card card;

        // --- public 메서드 ---

        /// <summary>
        /// 카드 데이터를 설정하고 UI를 갱신합니다.
        /// </summary>
        public virtual void SetCard(Card card)
        { 
            this.card = card;
            UpdateView();
        }

        public Card GetCurrentCard()
        {
            if(this.card is null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return this.card;
        }

        /// <summary>
        /// 카드의 모든 정보를 UI에 반영합니다.
        /// </summary>
        public void UpdateView()
        {
            // 카드 일러스트 및 경험치 게이지 표시
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);

            // 카드 이름, 설명, 레벨 표시
            cardNameText.text = card.cardName;
            var descParams = card.cardAction.GetDescriptionParams(card);
            descriptionText.text = FormatDescription(card.cardDescription, descParams);

            levelText.text = $"Lv.{card.cardEnhancement.level.Value}";
            
            // 속성 엠블럼 표시
            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = (propertyEmblemImage.sprite != null); // 없으면 비활성화
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
        
        // --- private 메서드 ---
        private string FormatDescription(string template, string[] descParams)
        {
            if (descParams == null || descParams.Length == 0)
                return template;

            string result = template;
            for (int i = 0; i < descParams.Length; i++)
            {
                result = result.Replace("{" + i + "}", descParams[i]);
            }//TODO : 성능 이슈 있으면 StringBuilder로 고치기
            return result;
        }
    }
}