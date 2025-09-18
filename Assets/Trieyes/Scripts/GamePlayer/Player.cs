using System.Collections.Generic;
using UnityEngine;
using CharacterSystem;
using RelicSystem;
using OutGame;
using CardSystem;
using UISystem;

namespace GamePlayer
{
    public class GameScoreRecoder
    {
        public int killScore = 0;
        public int roundScore = 0;
        public int goldScore = 0;
    }
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
        public GameScoreRecoder gameScoreRecoder = new GameScoreRecoder();

        [SerializeField] private GameObject developerUIPrefab;
        private bool isDeveloperUIActive = false;
        private GameObject currentDeveloperUI;

        public static Player Instance { get; private set; }

        public int mainCharacterId;

        public Card selectedCard;
        public AchievementData selectedRelic;

        
        [Header("스테이지 간 노드 개수 관리")]
        private int _StageNodeCount = 5;
        public int bossStageLeftCount = 0;

        private int _nextRoundMinNodeCount = 1;
        private int _nextRoundMaxNodeCount = 2;

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

            bossStageLeftCount = _StageNodeCount;
            
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

        public void SetNextStageNodeCount()
        {
            _StageNodeCount += Random.Range(_nextRoundMinNodeCount, _nextRoundMaxNodeCount + 1);
            bossStageLeftCount = _StageNodeCount;
        }

        // F12 버튼 누르면 개발자 UI 표시
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12))
            {
                if(isDeveloperUIActive)
                {
                    if (currentDeveloperUI != null)
                    {
                        Destroy(currentDeveloperUI);
                        currentDeveloperUI = null;
                    }
                    if (BattleOverlayCanvasController.Instance is not null) 
                    {
                        BattleOverlayCanvasController.Instance.joystick.gameObject.SetActive(true);
                    }
                    isDeveloperUIActive = false;
                    Time.timeScale = 1f;
                    return;
                }

                else
                {
                    if (BattleOverlayCanvasController.Instance is not null)
                    {
                        BattleOverlayCanvasController.Instance.joystick.gameObject.SetActive(false);
                    }
                    currentDeveloperUI = Instantiate(developerUIPrefab);
                    // Canvas 자식으로 등록
                    var canvas = GameObject.Find("Canvas");
                    currentDeveloperUI.transform.SetParent(canvas.transform);
                    Time.timeScale = 0f;
                    isDeveloperUIActive = true;
                    currentDeveloperUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
                }
            }
        }
    }
}