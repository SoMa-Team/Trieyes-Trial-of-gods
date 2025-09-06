using System;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Trieyes.Scripts.GameOver
{
    public class GameOverManager : MonoBehaviour
    {
        // ========= [싱글턴] =========
        public static GameOverManager Instance { get; private set; }

        // ========= [UI Prefab] =========
        [Header("GameOver UI")]
        public GameOverUIPrefab gameOverUIPrefab;

        // ========= [라이프사이클] =========
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            var rectTransform = gameObject.GetComponent<RectTransform>();

            // rect transform left, top, posz, right, bottom = 0
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
        }

        private void Start()
        {
            if (gameOverUIPrefab != null)
            {
                SetupUIEvents();
            }

            if (gameOverUIPrefab != null)
            {
                gameOverUIPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            gameObject.SetActive(false);
        }

        // ========= [UI 이벤트 설정] =========
        private void SetupUIEvents()
        {
            gameOverUIPrefab.OnBackToTitlePressed += OnMainMenuButtonPressed;
        }

        // ========= [활성화/비활성화] =========
        public void Activate()
        {
            Debug.Log("GameOverManager: Activate");

            // Canvas의 scale을 복원 (GameOverScene에서 scale이 0으로 설정되어 있음)
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.transform.localScale = Vector3.one;
            }

            if (gameObject != null)
            {
                gameObject.SetActive(true);
            }
            gameOverUIPrefab.ShowUI();
        }

        public void Deactivate()
        {
            Debug.Log("GameOverManager: Deactivate");
            gameOverUIPrefab.HideUI();
            
            // Canvas의 scale을 다시 0으로 설정 (숨김)
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                canvas.transform.localScale = Vector3.zero;
            }
            
            gameObject.SetActive(false);
        }

        // ========= [버튼 이벤트 핸들러] =========
        private void OnMainMenuButtonPressed()
        {
            Deactivate();
            // TODO: 메인 메뉴로 이동하는 로직 구현
            Debug.Log("Main Menu button pressed - implement main menu navigation");
        }
    }
}