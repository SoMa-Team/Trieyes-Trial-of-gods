using PrimeTween;
using UnityEngine;

public class CardTriggerSlot : MonoBehaviour
{
    [SerializeField] public RectTransform rectTransform;
    
    [SerializeField] private float RotateScale = 20;
    [SerializeField] private float SizeScale = 0.3f;
    
    public Tween TriggerCard(float duration)
    {
        return Tween.Custom(0f, 1f, duration, t =>
        {
            var rotateScale = RotateScale * (0.5f - Mathf.Abs(t - 0.5f));
            var rotateDegree = rotateScale * Mathf.Sin(t * 4 * Mathf.PI);
            var sizeScale = 1f + SizeScale * (0.5f - Mathf.Abs(t - 0.5f));
            
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotateDegree));
            transform.localScale = new Vector3(sizeScale, sizeScale, sizeScale);
            transform.SetAsLastSibling();
        });
    }
}
