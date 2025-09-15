using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.UI;

public class BattleWorldCanvasController : MonoBehaviour
{
    public static BattleWorldCanvasController Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;        
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    [SerializeField]
    public Slider hpBarSliderView;
    
    public void Activate()
    {
        hpBarSliderView.gameObject.SetActive(true);
    }

    public void Update()
    {
        if (BattleStage.now is null)
            return;
        
        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        var offset = new Vector3(0, -0.3f, 0);
        var character = BattleStage.now.mainCharacter;
        hpBarSliderView.value = character.HpRate;
        
        var targetPosition = character.transform.localPosition + offset;
        hpBarSliderView.transform.localPosition = targetPosition;
    }

    public void Deactivate()
    {
        hpBarSliderView.gameObject.SetActive(false);
    }
}
