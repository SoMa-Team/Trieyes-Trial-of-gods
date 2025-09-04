using UnityEngine;
using GamePlayer;
using CharacterSystem;
using UnityEngine.UI;

namespace OutGame{
    public class CharacterSelectView : MonoBehaviour
    {
        public CharacterSelectListView characterSelectListView;
        public GameObject characterIcon;
        public GameObject characterName;
        public GameObject characterSelectButton;

        public AchievementData character;
        
        // 초기화 메서드
        public void Awake()
        {
            if (characterIcon != null)
            {
                var button = characterIcon.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnCharacterIconClicked(null));
                }
            }
        }

        public void SetCharacterSelectListView(CharacterSelectListView characterSelectListView)
        {
            this.characterSelectListView = characterSelectListView;
        }

        public void OnCharacterIconClicked(AchievementData character)
        {
            Character selectedCharacter = Player.Instance.selectedCharacter;
            characterSelectListView.selectedCharacter = selectedCharacter;
            Debug.Log($"캐릭터 아이콘 클릭: {selectedCharacter.pawnName}");
        }
    }
}