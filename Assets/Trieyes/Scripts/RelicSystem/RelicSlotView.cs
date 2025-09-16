using System;
using JetBrains.Annotations;
using PrimeTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RelicSystem
{
    public class RelicSlotView : MonoBehaviour
    {
        [SerializeField] private Image IconImage;
        [SerializeField] private TextMeshProUGUI TitleText;
        [SerializeField] private TextMeshProUGUI DescriptionText;

        [SerializeField] private RectTransform innerTransform;

        private bool selected = false;
        public Relic Relic;
        [CanBeNull] private Action _onClickAction = null;

        public void Activate(Relic relic)
        {
            Relic = relic;

            if (relic.icon is not null)
                IconImage.sprite = relic.icon;
            TitleText.text = relic.name;
            DescriptionText.text = relic.description;
        }

        public void Deactivate()
        {
            selected = false;
            Relic = null;
            _onClickAction = null;
            innerTransform.localScale = Vector3.one;
        }

        public void SetOnClickAction(Action action)
        {
            _onClickAction = action;
        }

        public void OnClick()
        {
            _onClickAction?.Invoke();
        }

        public void SetSelected(bool selected)
        {
            AnimateScaleChange(selected);
            this.selected = selected;
        }

        private Tween AnimateScaleChange(bool selected)
        {
            float animationDuration = 0.2f;
            float selectScale = 1.3f;

            var startScale = this.selected ? selectScale : 1f;
            var endScale = selected ? selectScale : 1f;
            
            return Tween.Scale(innerTransform, startScale * Vector3.one, endScale * Vector3.one, animationDuration);
        }
    }
}