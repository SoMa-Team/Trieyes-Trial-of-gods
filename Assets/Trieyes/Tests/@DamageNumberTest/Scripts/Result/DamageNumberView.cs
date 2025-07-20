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
