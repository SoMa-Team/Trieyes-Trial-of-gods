using UnityEngine;
using GamePlayer;
using UnityEngine.UI;
using TMPro;

namespace OutGame{
    public class RelicSelectView : MonoBehaviour
    {
        public RelicSelectListView RelicSelectListView;

        public Button button;
        public AchievementData relicAchievementData;

        // RelicView의 기능을 통합
        public GameObject relicIcon;
        public GameObject relicName;
        public GameObject relicSelectButton;
        
        // 초기화 메서드
        public void Awake()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnButtonClicked());
            }
        }

        // RelicView의 Init 메서드 통합
        public void Init()
        {
            // 기본 상태로 초기화
            if (relicIcon != null)
            {
                relicIcon.SetActive(false);
            }
            
            if (relicName != null)
            {
                var relicNameComponent = relicName.GetComponent<TextMeshProUGUI>();
                if (relicNameComponent != null)
                {
                    relicNameComponent.text = "";
                }
            }
            
            if (relicSelectButton != null)
            {
                relicSelectButton.SetActive(false);
                
                var button = relicSelectButton.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = false;
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        public void SetRelicSelectListView(RelicSelectListView RelicSelectListView)
        {
            this.RelicSelectListView = RelicSelectListView;
        }

        public void SetSelectedRelic(AchievementData relic)
        {
            RelicSelectListView.selectedRelic = relic;
        }

        public void OnButtonClicked()
        {
            Debug.Log("유물 설명 : " + relicAchievementData.achievementDescription);
            RelicSelectListView.selectedRelic = relicAchievementData;
        }
    }
}
