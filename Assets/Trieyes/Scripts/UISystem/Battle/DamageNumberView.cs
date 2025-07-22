using System;
using AttackSystem;
using PrimeTween;
using TMPro;
using UnityEngine;

namespace UISystem
{
    public class DamageNumberView : MonoBehaviour
    {
        [NonSerialized] public RectTransform targetRectTransform;
        public TextMeshProUGUI text;

        private void Start()
        {
            text.outlineColor = Color.black;
            text.outlineWidth = 0.2f;
            text.outlineWidth = 0.2f;
            // TODO : PrimeTwin으로 점점 FadeOut 적용
        }

        // ===== 초기화 관련 =====
        public void SetDamage(AttackResult result)
        {
            text.text = result.totalDamage.ToString();
            if (result.isCritical)
            {
                text.color = Color.red;
            }
            else
            {
                text.color = Color.white;
            }
        }

        public void SetPosition(Vector3 position)
        {
            Vector3 localPoint = targetRectTransform.InverseTransformPoint(position);
        
            localPoint.y += 1; // TestOffset
            transform.SetParent(targetRectTransform);
            transform.localPosition = localPoint;
        }
    
        // ===== 초기화 관련 =====
        public virtual void Activate()
        {
            text.alpha = 1;
            Tween.Alpha(text, 0f, 2f).OnComplete(() =>
            {
                DamageNumberViewFactory.Instance.Deactivate(this);
            });
        }

        public virtual void Deactivate()
        {
        }
    }
}
