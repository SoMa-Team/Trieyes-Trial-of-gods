using UnityEngine;
using CardViews;
using UnityEngine.EventSystems;
using CardSystem;

namespace DeckViews
{
    public class CardInDeckView : CardView, IPointerClickHandler
    {
        public GameObject selectionOutline;

        public override void SetCard(Card card)
        {
            base.SetCard(card);
            SetSelected(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            DeckZoneManager.Instance.OnCardClicked(this);
        }

        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
            {
                selectionOutline.SetActive(selected);
            }
        }
    }
}