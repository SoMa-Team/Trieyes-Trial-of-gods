using UnityEngine;
using GamePlayer;
using UnityEngine.UI;

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
            relicView = GetComponent<RelicView>();

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnButtonClicked());
            }
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
