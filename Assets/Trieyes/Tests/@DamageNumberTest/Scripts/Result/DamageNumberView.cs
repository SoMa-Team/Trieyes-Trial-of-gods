using System;
using AttackSystem;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.WSA;

public class DamageNumberView : MonoBehaviour
{
    [NonSerialized] public RectTransform targetRectTransform;
    public TextMeshProUGUI text;

    private void Start()
    {
        text.outlineColor = Color.black;
        text.outlineWidth = 0.2f;
        // TODO : PrimeTwin으로 점점 FadeOut 적용
    }

    // ===== 초기화 관련 =====
    public void SetDamage(AttackResult result)
    {
        text.text = result.totalDamage.ToString();
    }

    public void SetPosition(Vector3 position)
    {
        Vector3 localPoint = targetRectTransform.InverseTransformPoint(position);
        
        transform.SetParent(targetRectTransform);
        transform.localPosition = localPoint;
    }

    public virtual void Activate()
    {
    }

    public virtual void Deactivate()
    {
    }
}
