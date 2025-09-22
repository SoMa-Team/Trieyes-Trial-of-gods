using System;
using UnityEngine;
using GameFramework;
using System.Threading.Tasks;
using RelicSystem;
using GamePlayer;
using PrimeTween;

namespace OutGame{
    public class StartSceneManager : MonoBehaviour
    {

        public Player player;

        public static StartSceneManager Instance { get; private set; }
        
        [SerializeField] public GameObject SafeAreaPanel;
        
        [SerializeField] private GameObject TitlePanel;
        private CanvasGroup TitleCanvasGroup;
        
        [SerializeField] private GameObject CharacterSelectPanel;
        private CanvasGroup CharacterSelectCanvasGroup;

        async void Start()
        {
            Debug.Log("StartSceneManager");
            await _Start();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        async Task _Start()
        {
            await RelicDataBase.InitializeAsync();
        }

        private void Awake()
        {
            InitializePanels();
        }

        /// <summary>
        /// 모든 패널을 초기 상태로 설정합니다.
        /// </summary>
        private void InitializePanels()
        {
            SetupCanvasGroup();
            ShowPanel(TitlePanel);
        }

        /// <summary>
        /// 패널에 CanvasGroup을 설정합니다.
        /// </summary>
        /// <param name="panel">설정할 패널</param>
        private void SetupCanvasGroup()
        {
            // SafeAreaPanel Rect Transform 하위로 2개의 게임 오브젝트 Instantiate
            if (SafeAreaPanel == null)
            {
                Debug.LogError("SafeAreaPanel이 할당되지 않았습니다. 인스펙터에서 할당해주세요.");
                return;
            }
            
            TitleCanvasGroup = TitlePanel.GetComponent<CanvasGroup>();
            if (TitleCanvasGroup == null)
            {
                TitleCanvasGroup = TitlePanel.AddComponent<CanvasGroup>();
            }

            CharacterSelectCanvasGroup = CharacterSelectPanel.GetComponent<CanvasGroup>();
            if (CharacterSelectCanvasGroup == null)
            {
                CharacterSelectCanvasGroup = CharacterSelectPanel.AddComponent<CanvasGroup>();
            }
            
            TitleCanvasGroup.alpha = 0f;
            TitleCanvasGroup.interactable = false;
            TitleCanvasGroup.blocksRaycasts = false;
            
            CharacterSelectCanvasGroup.alpha = 0f;
            CharacterSelectCanvasGroup.interactable = false;
            CharacterSelectCanvasGroup.blocksRaycasts = false;
            
            TitlePanel.SetActive(true);
            CharacterSelectPanel.SetActive(true);
        }

        public void GameStart()
        {
            SceneChangeManager.Instance.GameStart();
        }
        
        public void ToCharacterSelectPanel()
        {
            ShowPanel(CharacterSelectPanel);
        }

        public void CloseCharacterSelectPanel()
        {
            Player.Instance.mainCharacterId = -1;
            HidePanel(CharacterSelectPanel);
        }

        /// <summary>
        /// 패널을 Fade In 효과와 함께 표시합니다.
        /// </summary>
        /// <param name="panel">표시할 패널</param>
        private void ShowPanel(GameObject panel)
        {
            if (panel == null) return;
            
            // CanvasGroup 컴포넌트 가져오기 (없으면 추가)
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogError("CanvasGroup 컴포넌트가 없습니다. 패널에 CanvasGroup을 추가하세요.");
                return;
            }

            // Fade In 애니메이션 실행
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 패널을 Fade Out 효과와 함께 숨깁니다.
        /// </summary>
        /// <param name="panel">숨길 패널</param>
        private void HidePanel(GameObject panel)
        {
            if (panel == null) return;

            // CanvasGroup 컴포넌트 가져오기
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                // CanvasGroup이 없으면 즉시 비활성화
                panel.SetActive(false);
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void OnDestroy()
        {
            var cg = TitlePanel.GetComponent<CanvasGroup>();
            Destroy(cg.gameObject);

            cg = CharacterSelectPanel.GetComponent<CanvasGroup>();
            Destroy(cg.gameObject);
        }
    }
}