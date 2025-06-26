using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CardSystem
{
    /// <summary>
    /// 카드의 UI 표시를 담당하는 클래스입니다.
    /// 카드의 시각적 정보(이미지, 텍스트 등)를 관리하고 업데이트합니다.
    /// </summary>
    public class CardView : MonoBehaviour
    {
        // --- 필드 ---

        [Header("UI Reference")]
        /// <summary>
        /// 카드의 일러스트레이션을 표시하는 이미지 컴포넌트입니다.
        /// </summary>
        public Image illustrationImage;

        /// <summary>
        /// 카드의 스탯 정보를 표시하는 텍스트 컴포넌트입니다.
        /// </summary>
        public TextMeshProUGUI statText;

        /// <summary>
        /// 카드의 설명을 표시하는 텍스트 컴포넌트입니다.
        /// </summary>
        public TextMeshProUGUI descriptionText;

        /// <summary>
        /// 카드의 속성 아이콘 또는 배경을 표시하는 이미지 컴포넌트입니다.
        /// </summary>
        public Image propertyIconOrBG;

        /// <summary>
        /// 카드의 레벨과 경험치 정보를 표시하는 텍스트 컴포넌트입니다.
        /// </summary>
        public TextMeshProUGUI levelExpText;

        /// <summary>
        /// 이 뷰가 표시할 카드 객체입니다.
        /// </summary>
        public Card card;

        // --- public 메서드 ---

        /// <summary>
        /// 카드 뷰에 새로운 카드를 설정합니다.
        /// 카드가 설정되면 자동으로 뷰를 업데이트합니다.
        /// </summary>
        /// <param name="card">표시할 카드 객체</param>
        public void SetCard(Card card)
        {
            this.card = card;
            UpdateView();
        }

        /// <summary>
        /// 카드 뷰를 업데이트합니다.
        /// 현재 설정된 카드의 정보를 UI 요소에 반영합니다.
        /// </summary>
        public void UpdateView()
        {
            // TODO: 카드 정보를 UI 요소에 반영하는 로직 구현
            // - illustrationImage.sprite = card.cardActionSO.illustration;
            // - statText.text = 카드 스탯 정보;
            // - descriptionText.text = card.cardActionSO.cardDescription;
            // - propertyIconOrBG.sprite = 속성 아이콘;
            // - levelExpText.text = 레벨/경험치 정보;
        }
    }
}