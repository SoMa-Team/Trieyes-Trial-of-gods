using UnityEngine;
using TMPro;
using GamePlayer;
using UnityEngine.UI;
using GameFramework;
using RelicSystem;

namespace OutGame{
    public class RelicSelectView : MonoBehaviour
    {
        public RelicListView relicListView;

        public Button button;
        public AchievementData relicAchievementData;

        public RelicView relicView;
        
        // 초기화 메서드
        public void Awake()
        {
            button = GetComponent<Button>();
            relicView = GetComponent<RelicView>();
        }

        public void SetRelicListView(RelicListView relicListView)
        {
            this.relicListView = relicListView;
        }

        public void SetRelicView(RelicView relicView)
        {
            this.relicView = relicView;
        }

        public void SetSelectedRelic(AchievementData relic)
        {
            relicListView.selectedRelic = relic;
        }

        public void OnButtonClicked()
        {
            Debug.Log("유물 설명 : " + relicAchievementData.achievementDescription);
            relicListView.selectedRelic = relicAchievementData;
        }
    }
}
