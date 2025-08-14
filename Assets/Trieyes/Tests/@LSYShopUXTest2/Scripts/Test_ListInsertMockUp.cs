using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class Test_ListInsertMockUp : MonoBehaviour
{
    [SerializeField] private RectTransform[] DownObjects;
    [SerializeField] private RectTransform TriggerObject;
    [SerializeField] private Graphic[] TriggerGraphicObjects;

    public float Duration = 0.3f;
    public float Length = 47f;

    private void Start()
    {
        foreach (var obj in TriggerGraphicObjects)
        {
            Tween.Alpha(obj, 0f, 0f);
        }
    }

    public void Trigger()
    {
        var sequence = Sequence.Create();
        foreach (var downObject in DownObjects)
        {
            sequence.Group(Tween.UIAnchoredPositionY(downObject, downObject.anchoredPosition.y - Length, Duration,
                Ease.OutCubic));
        }
        sequence.Group(Tween.UIAnchoredPositionY(TriggerObject, TriggerObject.anchoredPosition.y + 4 * Length,
            Duration, Ease.OutCubic));

        foreach (var triggerGraphicObject in TriggerGraphicObjects)
        {
            sequence.Group(Tween.Alpha(triggerGraphicObject, 1f, Duration, Ease.OutCubic));
        }
    }
}
