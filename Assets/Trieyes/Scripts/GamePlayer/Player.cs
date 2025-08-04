using System.Collections.Generic;
using UnityEngine;
using CharacterSystem;
using RelicSystem;

namespace GamePlayer
{
    public class Player : MonoBehaviour
    {
        // ===== [기능 1] 유저 정보 =====
        public string userName;
        public int userId;
        // ... 기타 필드
        
        [Header("업적 시스템")]
        [SerializeField] private AchievementDatabaseSO achievementDatabase;
        public Achievement achievement;

        public static Player Instance { get; private set; }

        public Character selectedCharacter; // 캐릭터 선택은 일단 인스펙터에서 직접 할당
        public List<int> selectedRelicIds; // 유물 선택은 Achievement에서 연동 + 선택 창에서 id 받기

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(this);
            InitializeAchievement();
        }
        
        private void InitializeAchievement()
        {
            if (achievementDatabase != null)
            {
                // ScriptableObject에서 업적 데이터 로드
                achievement = new Achievement(achievementDatabase);
                Debug.Log($"Player: ScriptableObject에서 업적 데이터 로드 완료 - {achievement.achievementDictionary.Count}개");
            }
            else
            {
                // 폴백으로 CSV에서 로드
                achievement = new Achievement();
                Debug.LogWarning("Player: AchievementDatabaseSO가 할당되지 않아 CSV에서 로드합니다.");
            }
        }
        
        // 인스펙터에서 SO 할당 시 자동으로 Achievement 재초기화
        private void OnValidate()
        {
            if (achievementDatabase != null && achievement != null)
            {
                // 에디터에서만 실행되도록 체크
                if (Application.isPlaying)
                {
                    InitializeAchievement();
                }
            }
        }
        
        // 업적 관련 편의 메서드들
        public bool IsAchievementUnlocked(int achievementId)
        {
            return achievement?.IsAchievementUnlocked(achievementId) ?? false;
        }
        
        public AchievementData GetAchievementData(int achievementId)
        {
            return achievement?.GetAchievementData(achievementId);
        }
        
        public void UpdateAchievementProgress(int achievementId, int newProgress)
        {
            achievement?.UpdateAchievementProgress(achievementId, newProgress);
        }
        
        public void CheckAllAchievements()
        {
            achievement?.CheckAllAchievements();
        }
    }
}