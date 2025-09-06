using System.Collections.Generic;
using UnityEngine;
using CharacterSystem;
using RelicSystem;
using OutGame;
using CardSystem;

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
        
        [HideInInspector]
        public JsonToAchivement jsonToAchivement = new JsonToAchivement();

        public static Player Instance { get; private set; }

        public int mainCharacterId;
        public Card selectedCard;
        public AchievementData selectedRelic;

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