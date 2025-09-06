using System;
using GameFramework;
using UnityEngine;

namespace GameOver
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
                gameOverUIPrefab.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            gameObject.SetActive(false);
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

        public void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}