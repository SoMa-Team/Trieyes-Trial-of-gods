using UnityEngine;
using TMPro;
using GamePlayer;
using UnityEngine.UI;

namespace OutGame{
    public class RelicView : MonoBehaviour
    {
        public GameObject relicIcon;
        public GameObject relicName;
        public GameObject relicSelectButton;

        public AchievementData relic;
        
        // 초기화 메서드
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
    }
}