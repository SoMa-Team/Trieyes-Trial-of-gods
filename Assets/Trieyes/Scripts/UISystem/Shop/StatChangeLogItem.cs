using System;
using PrimeTween;
using Stats;
using TMPro;
using UnityEngine;
using Utils;

public class StatChangeLogItem : MonoBehaviour
{
    [SerializeField] public RectTransform rect;
    [SerializeField] private TextMeshProUGUI text;

    private int logItemHeight = 30;
    
    public void Activate(StatType statType, int value, StatModifier statModifier, int targetPosition)
    {
        rect.anchoredPosition = new Vector2(0, targetPosition * logItemHeight);
        
        int nextValue = statModifier.getNextValue(value);

        string color = (nextValue - value) switch
        {
            0 => "#888",
            _ when value > nextValue => "#88f",
            _ when value < nextValue => "#f88"
        };

        var changeValue = statModifier.value;
        var absValue = changeValue >= 0 ? changeValue : -changeValue;
        string signString = changeValue >= 0 ? "+" : "-";

        string modifierString = statModifier.operationType switch
        {
            BuffOperationType.Additive => $"{signString} {absValue}",
            BuffOperationType.Multiplicative => $"* {100 + changeValue}%",
            BuffOperationType.Set => $""
        };

        if (statModifier.operationType == BuffOperationType.Set)
        {
            text.text = $"{StatTypeTransformer.StatTypeToKorean(statType)} : {value} <color={color}>=> {nextValue}</color>";
        }
        else
        {
            text.text = $"{StatTypeTransformer.StatTypeToKorean(statType)} : {value} <color={color}>{modifierString}</color> = {nextValue}";   
        }
    }

    public Tween AnimateToPosition(float duration, int position)
    {
        return Tween.UIAnchoredPositionY(rect, -position * logItemHeight, duration);
    }

    public void Deactivate()
    {
        
    }
}
