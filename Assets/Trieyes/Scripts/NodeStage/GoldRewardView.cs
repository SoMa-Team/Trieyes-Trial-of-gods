using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace NodeStage
{
    public class GoldRewardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private Image selectionOutline;
        
        private Action<GoldRewardView> onClicked;
        public int Amount { get; private set; }
        
        public void Activate(int amount, Action<GoldRewardView> onClicked)
        {
            Amount = amount;
            this.onClicked = onClicked;
            if (amountText) amountText.text = $"+{amount} Gold";
            SetSelected(false);
        }
        
        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
            {
                selectionOutline.enabled = selected; // or change color
                selectionOutline.color = selected ? Color.yellow : 
                    new Color(246f/255f, 220f/255f, 168f/255f, 0f);
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            onClicked?.Invoke(this);
        }
    }
}