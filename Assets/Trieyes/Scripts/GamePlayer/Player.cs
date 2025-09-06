using System.Collections.Generic;
using UnityEngine;
using CharacterSystem;
using RelicSystem;
using OutGame;
using CardSystem;
using GameFramework;

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

        public JsonToAchivement jsonToAchivement;
        public PlayScoreLogger playScoreLogger;

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
            Activate();
        }

        public void Activate()
        {
            InitializeAchievement();
            playScoreLogger = new PlayScoreLogger();

            if (!gameObject.GetComponent<JsonToAchivement>())
            {
                Debug.LogError("JsonToAchivement not found");
            }
            else
            {
                jsonToAchivement = gameObject.GetComponent<JsonToAchivement>();
            }
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

        public void Deactivate()
        {
            playScoreLogger.ResetScore();
            mainCharacterId = 0;
            selectedCard = null;
            selectedRelic = null;
        }
    }
}