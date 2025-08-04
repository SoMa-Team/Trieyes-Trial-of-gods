using System;
using System.Collections.Generic;
using CharacterSystem;
using Stats;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.WSA;
using Utils;

public class NewShopSceneManager : MonoBehaviour
{
    public static NewShopSceneManager Instance { get; private set; }

    void Awake()
    {
        if (Instance is not null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    
    private Character mainCharacter;
    private Difficulty difficulty;

    public void Activate(Character mainCharacter, Difficulty difficulty)
    {
        this.mainCharacter = mainCharacter;
        this.difficulty = difficulty;
        
        UpdatePlayerRelics();
        OnScreenResized();
    }

    public void Deactivate()
    {
    }

    [SerializeField] private TextMeshProUGUI textRoundInfo;

    [SerializeField] private TextMeshProUGUI textGold;

    [SerializeField] private List<Image> imageRelics;

    [SerializeField] private GameObject popupStatInfo;

    [Serializable]
    class StatTypeTMPPair
    {
        public StatType statType;
        public List<TextMeshProUGUI> text;
    }

    [SerializeField] private StatTypeTMPPair[] statTypeTMPPairs;

    private void Update()
    {
        UpdateRoundInfo();
        UpdatePlayerGold();
        UpdatePlayerStat();
        CheckScreenResize();
    }

    private void UpdateRoundInfo()
    {
        // TODO : Stage, Round 구분하는 기능 Difficulty 업데이트 이후 추가해야함.
        textRoundInfo.text = $"Stage {difficulty.stageNumber} - <color=#ff9>Shop</color> {1}";
    }

    private void UpdatePlayerGold()
    {
        // TODO : 골드 출력 서식을 적용할 것인지 확인 필요 (예시> 3자리 마다 ',' 표시)
        textGold.text = $"{mainCharacter.gold}";
    }

    private void UpdatePlayerRelics()
    {
        for (int i = 0; i < mainCharacter.relics.Count; i++)
        {
            var relic = mainCharacter.relics[i];
            if (i >= imageRelics.Count)
            {
                throw new Exception($"ShopSceneManager : Relic 아이콘을 출력할 공간이 부족합니다.");
            }
            
            var relicView = imageRelics[i];

            if (relic.icon is null)
            {
                Debug.Log($"ShopSceneManager : Relic({relic.name})의 아이콘이 없습니다.");
            }
            relicView.sprite = relic.icon;
        }
    }

    private void UpdatePlayerStat()
    {
        foreach (var statTypeTMPPair in statTypeTMPPairs)
        {
            var statType = statTypeTMPPair.statType;
            foreach (var tmp in statTypeTMPPair.text)
            {
                tmp.text = mainCharacter.statSheet[statType].Value.ToString();   
            }
        }
    }


    private int lastScreenWidth;
    private int lastScreenHeight;
    private void CheckScreenResize()
    {
        if (lastScreenWidth == Screen.width && lastScreenHeight == Screen.height)
            return;
        
        OnScreenResized();
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    // ===== Event Listeners =====


    public void OnClickNextRound()
    {
        Debug.Log("OnClickNextRound");
    }

    public void OnClickStatInfo()
    {
        ToggleStatInfoPopup();
    }

    private void ToggleStatInfoPopup()
    {
        var isActivate = popupStatInfo.activeSelf;
        popupStatInfo.SetActive(!isActivate);
    }

    [SerializeField] private RectTransform DeckScaleRect;
    [SerializeField] private RectTransform ShopScaleRect;
    private void OnScreenResized()
    {
        // float deckScale = DeckScaleRect;
        // float shopScale;

        // DeckScaleRect.localScale = Vector3.one * deckScale;
        // ShopScaleRect.localScale = Vector3.one * shopScale;
    }
}
