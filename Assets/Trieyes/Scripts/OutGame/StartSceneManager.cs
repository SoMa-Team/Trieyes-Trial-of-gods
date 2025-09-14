using UnityEngine;
using CharacterSystem;
using GameFramework;
using System.Threading.Tasks;
using RelicSystem;
using GamePlayer;
using PrimeTween;
using CardSystem;

namespace OutGame{
    public class StartSceneManager : MonoBehaviour
    {

        public Player player;

        public static StartSceneManager Instance { get; private set; }
        
        public GameObject CharacterSelectPanel;

        async void Start()
        {
            Debug.Log("StartSceneManager");
            InitializePanels();
            await _Start();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        async Task _Start()
        {
            await RelicDataBase.InitializeAsync();
        }

        /// <summary>
        /// 모든 패널을 초기 상태로 설정합니다.
        /// </summary>
        public void InitializePanels()
        {
            // 패널들을 비활성화하고 CanvasGroup 설정
            if (CharacterSelectPanel != null)
            {
                CharacterSelectPanel.SetActive(false);
                SetupCanvasGroup(CharacterSelectPanel);
            }
        }

        /// <summary>
        /// 패널에 CanvasGroup을 설정합니다.
        /// </summary>
        /// <param name="panel">설정할 패널</param>
        private void SetupCanvasGroup(GameObject panel)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }
            canvasGroup.alpha = 0f; // 초기 알파값을 0으로 설정
        }

        public void GameStart()
        {
            SceneChangeManager.Instance.GameStart();
        }
        
        public void ToCharacterSelectPanel()
        {
            ShowPanelWithFade(CharacterSelectPanel);
        }

        public void CloseCharacterSelectPanel()
        {
            Player.Instance.mainCharacterId = -1;
            HidePanelWithFade(CharacterSelectPanel);
        }

        /// <summary>
        /// 패널을 Fade In 효과와 함께 표시합니다.
        /// </summary>
        /// <param name="panel">표시할 패널</param>
        private void ShowPanelWithFade(GameObject panel)
        {
            if (panel == null) return;

            // 패널을 활성화
            panel.SetActive(true);
            
            // CanvasGroup 컴포넌트 가져오기 (없으면 추가)
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.AddComponent<CanvasGroup>();
            }

            // 초기 알파값을 0으로 설정
            canvasGroup.alpha = 0f;
            
            // Fade In 애니메이션 실행
            Tween.Alpha(canvasGroup, 1f, 0.3f, Ease.OutQuad);
        }

        /// <summary>
        /// 패널을 Fade Out 효과와 함께 숨깁니다.
        /// </summary>
        /// <param name="panel">숨길 패널</param>
        private void HidePanelWithFade(GameObject panel)
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

            // Fade Out 애니메이션 실행 후 패널 비활성화
            Tween.Alpha(canvasGroup, 0f, 0.3f, Ease.InQuad)
                .OnComplete(() => panel.SetActive(false));
        }
    }
}