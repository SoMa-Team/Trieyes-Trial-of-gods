using GamePlayer;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace OutGame{
    public class CharacterSelectListView : ListView
    {
        public CharacterSelectView characterSelectView;
        public IAchievementObject selectedCharacterAchievement;
        
        // JsonToAchivement 연동
        private JsonToAchivement jsonToAchivement;
        private List<IAchievementObject> characterAchievements;
        
        // 현재 선택된 캐릭터 인덱스
        private int currentCharacterIndex = 0;
        
        // 생성된 캐릭터 뷰들
        private List<CharacterSelectView> characterViews = new List<CharacterSelectView>();
        
        [Header("UI References")]
        public Transform characterListParent; // 캐릭터 뷰들이 생성될 부모 Transform
        
        [Header("Character View Prefabs")]
        public GameObject unlockedCharacterViewPrefab; // 해금된 캐릭터용 프리팹
        public GameObject lockedCharacterViewPrefab; // 잠긴 캐릭터용 프리팹

        [Header("Button")]
        public Button PrevButton;
        public Button NextButton;
        
        // 초기화 메서드
        public void Awake()
        {
            // JsonToAchivement 참조 가져오기
            jsonToAchivement = Player.Instance.jsonToAchivement;
            
            if (jsonToAchivement != null)
            {
                // 캐릭터 업적 데이터 로드
                characterAchievements = jsonToAchivement.GetAchievementsByType(JsonToAchivement.CHARACTER_TYPE);
                Debug.Log($"캐릭터 업적 {characterAchievements.Count}개 로드됨");
                
                // 모든 캐릭터 뷰 생성
                CreateCharacterList();
                
                // 첫 번째 캐릭터로 초기화
                if (characterAchievements.Count > 0)
                {
                    currentCharacterIndex = 0;
                    UpdateCurrentCharacter();
                }
            }
            else
            {
                Debug.LogError("JsonToAchivement를 찾을 수 없습니다.");
            }
            
            if (characterSelectView != null)
            {
                characterSelectView.SetCharacterSelectListView(this);
            }


            if (PrevButton != null)
            {
                PrevButton.onClick.RemoveAllListeners();
                PrevButton.onClick.AddListener(() => OnPrevButtonClicked());
            }

            if (NextButton != null)
            {
                NextButton.onClick.RemoveAllListeners();
                NextButton.onClick.AddListener(() => OnNextButtonClicked());
            }
        }

        /// <summary>
        /// 캐릭터 업적 데이터를 반환합니다.
        /// </summary>
        /// <returns>캐릭터 업적 리스트</returns>
        public List<IAchievementObject> GetCharacterAchievements()
        {
            return characterAchievements;
        }

        /// <summary>
        /// 해금된 캐릭터만 반환합니다.
        /// </summary>
        /// <returns>해금된 캐릭터 업적 리스트</returns>
        public List<IAchievementObject> GetUnlockedCharacters()
        {
            if (jsonToAchivement != null)
            {
                return jsonToAchivement.GetUnlockedAchievementsByType(JsonToAchivement.CHARACTER_TYPE);
            }
            return new List<IAchievementObject>();
        }

        /// <summary>
        /// 특정 캐릭터를 선택합니다.
        /// </summary>
        /// <param name="characterAchievement">선택할 캐릭터 업적</param>
        public void SelectCharacter(IAchievementObject characterAchievement)
        {
            if (characterAchievement != null && characterAchievement.IsUnlocked)
            {
                // 캐릭터 업적 데이터만 저장 (실제 Character 객체는 게임 시작 시 생성)
                selectedCharacterAchievement = characterAchievement;
                Debug.Log($"캐릭터 선택: {characterAchievement.Name} (ID: {characterAchievement.Id})");
            }
            else
            {
                Debug.LogWarning("해금되지 않은 캐릭터를 선택할 수 없습니다.");
            }
        }

        /// <summary>
        /// 캐릭터 리스트 UI를 생성합니다.
        /// </summary>
        private void CreateCharacterList()
        {
            if (characterListParent == null || unlockedCharacterViewPrefab == null || lockedCharacterViewPrefab == null)
            {
                Debug.LogWarning("UI 설정이 완료되지 않았습니다.");
                return;
            }

            // 기존 뷰들 제거
            foreach (Transform child in characterListParent) Destroy(child.gameObject);
            characterViews.Clear();

            // 각 캐릭터에 대해 UI 생성
            for (int i = 0; i < characterAchievements.Count; i++)
            {
                var characterAchievement = characterAchievements[i];
                GameObject prefabToUse = characterAchievement.IsUnlocked ? 
                    unlockedCharacterViewPrefab : lockedCharacterViewPrefab;
                
                GameObject characterViewObj = Instantiate(prefabToUse, characterListParent);
                CharacterSelectView characterView = characterViewObj.GetComponent<CharacterSelectView>();
                
                if (characterView != null)
                {
                    characterView.SetCharacterSelectListView(this);
                    characterView.SetCharacterAchievement(characterAchievement);
                    characterViews.Add(characterView);
                    
                    // 처음에는 모두 비활성화
                    characterViewObj.SetActive(false);
                }
            }

            Debug.Log($"캐릭터 리스트 UI 생성 완료: {characterViews.Count}개");
        }

        /// <summary>
        /// 캐릭터 리스트를 새로고침합니다.
        /// </summary>
        public void RefreshCharacterList()
        {
            if (jsonToAchivement != null)
            {
                characterAchievements = jsonToAchivement.GetAchievementsByType(JsonToAchivement.CHARACTER_TYPE);
                CreateCharacterList();
            }
        }

        /// <summary>
        /// 특정 캐릭터의 진행도를 업데이트합니다.
        /// </summary>
        /// <param name="characterId">캐릭터 ID</param>
        /// <param name="conditionType">조건 타입</param>
        /// <param name="increment">증가량</param>
        public void UpdateCharacterProgress(int characterId, UnlockConditionType conditionType, int increment = 1)
        {
            if (jsonToAchivement != null)
            {
                IAchievementObject character = jsonToAchivement.GetAchievementById(characterId);
                if (character != null)
                {
                    jsonToAchivement.UpdateProgress(character, conditionType, increment);
                    
                    // UI 새로고침
                    RefreshCharacterList();
                }
            }
        }

        /// <summary>
        /// 선택된 캐릭터 업적 데이터를 반환합니다.
        /// </summary>
        /// <returns>선택된 캐릭터 업적 데이터</returns>
        public IAchievementObject GetSelectedCharacterAchievement()
        {
            return selectedCharacterAchievement;
        }

        /// <summary>
        /// 선택된 캐릭터 ID를 반환합니다.
        /// </summary>
        /// <returns>선택된 캐릭터 ID</returns>
        public int GetSelectedCharacterId()
        {
            return selectedCharacterAchievement?.Id ?? -1;
        }

        public void ToCardSelectPanel()
        {
            if (selectedCharacterAchievement != null)
            {
                // 선택된 캐릭터 업적 데이터를 Player에 저장
                Player.Instance.mainCharacterId = selectedCharacterAchievement.Id;
                Debug.Log($"캐릭터 선택 완료: {selectedCharacterAchievement.Name} (ID: {selectedCharacterAchievement.Id})");
                
                StartSceneManager.Instance.ToCardSelectPanel();
            }
            else
            {
                Debug.LogWarning("캐릭터를 선택하지 않았습니다.");
            }
        }

        /// <summary>
        /// 이전 캐릭터로 이동합니다.
        /// </summary>
        public void OnPrevButtonClicked()
        {
            if (characterAchievements == null || characterAchievements.Count == 0) return;

            currentCharacterIndex--;
            if (currentCharacterIndex < 0)
            {
                currentCharacterIndex = characterAchievements.Count - 1; // 마지막 캐릭터로 순환
            }

            UpdateCurrentCharacter();
        }

        /// <summary>
        /// 다음 캐릭터로 이동합니다.
        /// </summary>
        public void OnNextButtonClicked()
        {
            if (characterAchievements == null || characterAchievements.Count == 0) return;

            currentCharacterIndex++;
            if (currentCharacterIndex >= characterAchievements.Count)
            {
                currentCharacterIndex = 0; // 첫 번째 캐릭터로 순환
            }

            UpdateCurrentCharacter();
        }

        /// <summary>
        /// 현재 선택된 캐릭터를 업데이트합니다.
        /// </summary>
        private void UpdateCurrentCharacter()
        {
            if (characterAchievements == null || currentCharacterIndex >= characterAchievements.Count) return;

            // 모든 뷰 비활성화
            foreach (var view in characterViews) view.gameObject.SetActive(false);
            
            // 현재 뷰만 활성화
            if (currentCharacterIndex < characterViews.Count)
            {
                characterViews[currentCharacterIndex].gameObject.SetActive(true);
            }

            var currentCharacter = characterAchievements[currentCharacterIndex];
            selectedCharacterAchievement = currentCharacter;

            Debug.Log($"현재 캐릭터: {currentCharacter.Name} (인덱스: {currentCharacterIndex})");
        }

        /// <summary>
        /// 현재 선택된 캐릭터 인덱스를 반환합니다.
        /// </summary>
        /// <returns>현재 캐릭터 인덱스</returns>
        public int GetCurrentCharacterIndex()
        {
            return currentCharacterIndex;
        }

        /// <summary>
        /// 특정 인덱스의 캐릭터로 이동합니다.
        /// </summary>
        /// <param name="index">이동할 캐릭터 인덱스</param>
        public void SetCurrentCharacterIndex(int index)
        {
            if (characterAchievements == null || characterAchievements.Count == 0) return;
            if (index < 0 || index >= characterAchievements.Count) return;

            currentCharacterIndex = index;
            UpdateCurrentCharacter();
        }
    }
}