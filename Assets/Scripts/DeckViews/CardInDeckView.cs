using UnityEngine;
using CardViews;
using UnityEngine.EventSystems;
using CardSystem;

namespace DeckViews
{
    /// <summary>
    /// 내 덱의 카드 뷰. 카드 선택/강조 및 클릭 이벤트 처리
    /// </summary>
    public class CardInDeckView : CardView, IPointerClickHandler
    {
        public GameObject selectionOutline;

        /// <summary>
        /// 카드 정보를 UI에 반영하고 선택 효과는 해제합니다.
        /// </summary>
        public override void SetCard(Card card)
        {
            base.SetCard(card);
            SetSelected(false);
        }

        /// <summary>
        /// 카드 클릭 시 DeckZoneManager에 알립니다.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            DeckZoneManager.Instance.OnCardClicked(this);
        }

        /// <summary>
        /// 카드가 선택되었는지 여부에 따라 강조 효과를 토글합니다.
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
            {
                selectionOutline.SetActive(selected);
            }
        }
    }
}