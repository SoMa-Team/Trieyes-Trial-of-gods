using CharacterSystem;

namespace OutGame{
    public class CharacterSelectListView : ListView
    {
        public CharacterSelectView characterSelectView;
        public Character selectedCharacter;
        
        // 초기화 메서드
        public void Awake()
        {
            if (characterSelectView != null)
            {
                characterSelectView.SetCharacterSelectListView(this);
            }
        }

        public void ToCardSelectPanel()
        {
            StartSceneManager.Instance.mainCharacter = selectedCharacter;
            StartSceneManager.Instance.ToCardSelectPanel();
        }
    }
}