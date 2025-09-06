using System;
using GameFramework;
using GamePlayer;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameOver
{
    public class GameOverUIPrefab : MonoBehaviour
    {
        // ========= [UI 요소들] =========
        [Header("GameOver UI Elements")]
        public GameObject killScorePanel;
        public GameObject moneyScorePanel;

        public GameObject TotalScorePanel;

        public GameObject ScoreImagePanel;

        public Button BackToTitleButton;

        public Sprite[] scoreImages;

        //TODO : Difficulty 등으로 옮기기
        private int totalScore;
        private int rankALowerBound = 1000;
        private int rankSLowerBound = 2000;

        // ========= [라이프사이클] =========
        private void Awake()
        {
            if (BackToTitleButton != null)
            {
                BackToTitleButton.onClick.RemoveAllListeners();
                BackToTitleButton.onClick.AddListener(OnBackToTitleButtonPressed);
            }
        }

        // ========= [버튼 이벤트 핸들러] =========
        private void OnBackToTitleButtonPressed()
        {
            Debug.Log("ChangeGameOverToGameStart");
            SceneChangeManager.Instance.ChangeGameOverToGameStart();
        }

        // ========= [UI 활성화/비활성화] =========
        public void ShowUI()
        {
            gameObject.SetActive(true);

            var killScore = Player.Instance.playScoreLogger.GetKillScore();
            var moneyScore = Player.Instance.playScoreLogger.GetMoneyScore();
            var totalScore = Player.Instance.playScoreLogger.GetTotalScore();

            SetKillScore(killScore);
            SetMoneyScore(moneyScore);
            SetTotalScore(totalScore);
            SetScoreImages();
        }

        public void HideUI()
        {
            gameObject.SetActive(false);
        }

        public void SetKillScore(int killScore)
        {
            killScorePanel.SetActive(true);

            var killScoreText = killScorePanel.GetComponentInChildren<TextMeshProUGUI>();
            killScoreText.text = "KILL SCORE :" + killScore.ToString();
        }

        public void SetMoneyScore(int moneyScore)
        {
            moneyScorePanel.SetActive(true);
            var moneyScoreText = moneyScorePanel.GetComponentInChildren<TextMeshProUGUI>();
            moneyScoreText.text = "MONEY SCORE :" + moneyScore.ToString();
        }

        public void SetTotalScore(int totalScore)
        {
            TotalScorePanel.SetActive(true);
            var totalScoreText = TotalScorePanel.GetComponentInChildren<TextMeshProUGUI>();
            totalScoreText.text = "TOTAL SCORE :" + totalScore.ToString();
            this.totalScore = totalScore;
        }

        public void SetScoreImages()
        {
            var img = ScoreImagePanel.GetComponent<Image>();
            var rank = totalScore > rankALowerBound ? totalScore > rankSLowerBound ? 2 : 1 : 0;

            // Set Native Size
            img.SetNativeSize();

            // ScoreImagePanel의 RectTransform을 가져와서 Anchor를 0.5, 0.5로 설정
            var rectTransform = ScoreImagePanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = new Vector2(560f, 0f);
            }

            img.sprite = scoreImages[rank];
        }
    }
}
