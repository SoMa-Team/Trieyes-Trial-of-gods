using UnityEngine;
using GamePlayer;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace OutGame{
    public class CharacterSelectView : MonoBehaviour
    {
        public CharacterSelectListView characterSelectListView;
        
        [Header("Unlocked Character UI")]
        public GameObject characterIcon;
        public GameObject characterName;
        public GameObject characterDescription;
        
        [Header("Locked Character UI")]
        public GameObject lockedCharacterIcon;
        public GameObject lockedCharacterName;
        public GameObject unlockProgressBar;
        public GameObject lockIcon;
        public GameObject unlockConditionText;

        [Header("Character Skill Info UI")]
        public SkillView skillView;

        // JsonToAchivement 연동
        private IAchievementObject characterAchievement;
        private JsonToAchivement jsonToAchivement;
        
        // 초기화 메서드
        public void Awake()
        {
            // JsonToAchivement 참조 가져오기
            jsonToAchivement = Player.Instance.jsonToAchivement;
            
            if (characterIcon != null)
            {
                var button = characterIcon.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnCharacterIconClicked());
                }
            }
        }

        public void SetCharacterSelectListView(CharacterSelectListView characterSelectListView)
        {
            this.characterSelectListView = characterSelectListView;
        }

        /// <summary>
        /// 캐릭터 업적 데이터를 설정합니다.
        /// </summary>
        /// <param name="achievement">캐릭터 업적 데이터</param>
        public void SetCharacterAchievement(IAchievementObject achievement)
        {
            characterAchievement = achievement;
            UpdateUI();
        }

        /// <summary>
        /// UI를 업데이트합니다.
        /// </summary>
        private void UpdateUI()
        {
            if (characterAchievement == null) return;

            bool isUnlocked = characterAchievement.IsUnlocked;

            // 해금 상태에 따라 다른 UI 요소 사용
            if (isUnlocked)
            {
                UpdateUnlockedUI();
            }
            else
            {
                UpdateLockedUI();
            }
        }

        /// <summary>
        /// 해금된 캐릭터 UI를 업데이트합니다.
        /// </summary>
        private void UpdateUnlockedUI()
        {
            // 해금된 캐릭터 UI 활성화
            if (characterIcon != null) characterIcon.SetActive(true);
            if (characterName != null) characterName.SetActive(true);
            if (characterDescription != null) characterDescription.SetActive(true);

            // 잠긴 캐릭터 UI 비활성화
            if (lockedCharacterIcon != null) lockedCharacterIcon.SetActive(false);
            if (lockedCharacterName != null) lockedCharacterName.SetActive(false);
            if (unlockProgressBar != null) unlockProgressBar.SetActive(false);
            if (lockIcon != null) lockIcon.SetActive(false);
            if (unlockConditionText != null) unlockConditionText.SetActive(false);

            // 캐릭터 이름 설정
            if (characterName != null)
            {
                var nameText = characterName.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = characterAchievement.Name;
                }
            }

            // 캐릭터 설명 설정
            if (characterDescription != null)
            {
                var descText = characterDescription.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                {
                    descText.text = characterAchievement.Description;
                }
            }

            // 스킬 정보 표시
            if (skillView != null)
            {
                skillView.UpdateSkillInfo(characterAchievement);
            }
        }

        /// <summary>
        /// 잠긴 캐릭터 UI를 업데이트합니다.
        /// </summary>
        private void UpdateLockedUI()
        {
            // 잠긴 캐릭터 UI 활성화
            if (lockedCharacterIcon != null) lockedCharacterIcon.SetActive(true);
            if (lockedCharacterName != null) lockedCharacterName.SetActive(true);
            if (unlockProgressBar != null) unlockProgressBar.SetActive(true);
            if (lockIcon != null) lockIcon.SetActive(true);
            if (unlockConditionText != null) unlockConditionText.SetActive(true);

            // 해금된 캐릭터 UI 비활성화
            if (characterIcon != null) characterIcon.SetActive(false);
            if (characterName != null) characterName.SetActive(false);
            if (characterDescription != null) characterDescription.SetActive(false);

            // 잠긴 캐릭터 이름 설정
            if (lockedCharacterName != null)
            {
                var nameText = lockedCharacterName.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                {
                    nameText.text = characterAchievement.Name;
                }
            }

            // 해금 조건 텍스트 설정
            if (unlockConditionText != null)
            {
                var conditionText = unlockConditionText.GetComponent<TextMeshProUGUI>();
                if (conditionText != null)
                {
                    conditionText.text = characterAchievement.UnlockConditionDescription;
                }
            }

            // 진행도 바 업데이트
            UpdateProgressBar();

            // 스킬 정보 숨기기 (잠긴 캐릭터는 스킬 정보 표시 안함)
            if (skillView != null)
            {
                skillView.HideSkillInfo();
            }
        }

        /// <summary>
        /// 진행도 바를 업데이트합니다.
        /// </summary>
        private void UpdateProgressBar()
        {
            if (unlockProgressBar == null || characterAchievement == null) return;

            var slider = unlockProgressBar.GetComponent<Slider>();
            if (slider != null && characterAchievement.UnlockProgress.Count > 0)
            {
                var progress = characterAchievement.UnlockProgress[0]; // 첫 번째 진행도 사용
                slider.value = (float)progress.currentValue / progress.maxValue;
            }
        }

        /// <summary>
        /// 캐릭터 아이콘 클릭 이벤트 (현재 캐릭터 선택)
        /// </summary>
        public void OnCharacterIconClicked()
        {
            if (characterAchievement == null) return;

            if (characterAchievement.IsUnlocked)
            {
                // 해금된 캐릭터 선택
                if (characterSelectListView != null)
                {
                    characterSelectListView.SelectCharacter(characterAchievement);
                }
                Debug.Log($"캐릭터 선택: {characterAchievement.Name}");
            }
            else
            {
                // 미해금 캐릭터 정보 표시
                ShowUnlockInfo();
            }
        }

        /// <summary>
        /// 해금 정보를 표시합니다.
        /// </summary>
        private void ShowUnlockInfo()
        {
            Debug.Log($"=== {characterAchievement.Name} 해금 정보 ===");
            Debug.Log($"설명: {characterAchievement.Description}");
            Debug.Log($"해금 조건: {characterAchievement.UnlockConditionDescription}");
            
            for (int i = 0; i < characterAchievement.UnlockProgress.Count; i++)
            {
                var progress = characterAchievement.UnlockProgress[i];
                Debug.Log($"진행도: {progress.key} - {progress.currentValue}/{progress.maxValue}");
            }
        }


        /// <summary>
        /// 진행도를 업데이트합니다.
        /// </summary>
        /// <param name="conditionType">조건 타입</param>
        /// <param name="increment">증가량</param>
        public void UpdateProgress(UnlockConditionType conditionType, int increment = 1)
        {
            if (jsonToAchivement != null && characterAchievement != null)
            {
                jsonToAchivement.UpdateProgress(characterAchievement, conditionType, increment);
                UpdateUI(); // 전체 UI 업데이트
            }
        }
    }
}