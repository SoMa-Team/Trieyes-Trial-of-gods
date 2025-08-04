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
            }
            else
            {
                // 폴백으로 CSV에서 로드
                achievement = new Achievement();
            }
        }
    }
}