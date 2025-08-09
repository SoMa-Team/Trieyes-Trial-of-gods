using System.Collections.Generic;
using System.IO;
using PrimeTween;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class StatItemInBattleStartPopupView : MonoBehaviour
{
    [SerializeField] public RectTransform rect;
    [SerializeField] private Image imageStatIcon;
    [SerializeField] private TextMeshProUGUI textStatName;
    [SerializeField] private TextMeshProUGUI textStatValue;
    [SerializeField] private Image imageIsActivate;
    [SerializeField] private List<Graphic> graphics;
    
    private StatType statType;
    private int statValue;
    private bool isActivate;
    
    public void Activate(StatType statType, int statValue, bool isActivate)
    {
        this.statType = statType;
        this.statValue = statValue;
        this.isActivate = isActivate;
        Invalidate();
        gameObject.SetActive(true);
    }

    public void TriggerModifier(StatModifier modifier, bool isActivate)
    {
        this.isActivate = isActivate;
        switch (modifier.operationType)
        {
            case BuffOperationType.Additive:
                statValue += modifier.value;
                break;
            case BuffOperationType.Multiplicative:
                statValue = statValue * (100 + modifier.value) / 100;
                break;
            case BuffOperationType.Set:
                statValue = modifier.value;
                break;
        }
        
        Invalidate();
    }

    private void Invalidate()
    {
        imageStatIcon.sprite = GetStatTypeSprite(statType);
        
        textStatName.text = GetStatTypeName(statType);
        textStatName.color = isActivate ? new Color(1, 1, 0.5f) : Color.white;
        
        textStatValue.text = statValue.ToString();
        textStatValue.color = isActivate ? new Color(1, 1, 0.5f) : Color.white;
        
        imageIsActivate.gameObject.SetActive(isActivate);
    }

    private string GetStatTypeName(StatType statType)
    {
        return StatTypeTransformer.StatTypeToKorean(statType);
    }

    private Sprite GetStatTypeSprite(StatType statType)
    {
        // TODO : 스탯 이미지 연결하기 & 현재는 이미지가 변경되지 않음
        return imageStatIcon.sprite;
    }

    public Sequence CreateAlphaSequence(float duration, float targetAlpha)
    {
        var sequence = Sequence.Create();
        foreach (var graphic in graphics)
        {
            sequence.Group(Tween.Alpha(graphic, targetAlpha, duration, Ease.Linear));
        }

        return sequence;
    }

    public void TriggerEnd()
    {
        isActivate = false;
        Invalidate();
    }
}
