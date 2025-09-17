using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GamePlayer;
using GameFramework;

namespace NodeStage
{
    public class GameOverStage : EventStage<GameOverStage>
    {
        [Header("UI")]
        [SerializeField] private GameObject killScoreText;
        [SerializeField] private GameObject roundScoreText;
        [SerializeField] private GameObject goldScoreText;
        [SerializeField] private GameObject totalScoreText;

        [SerializeField] private Sprite[] rankImages;

        [SerializeField] private GameObject RankImagePanel;
        [SerializeField] private Button backToGameStartButton;

        protected override void OnActivated()
        {
            killScoreText.GetComponent<TextMeshProUGUI>().text = "Kill: " + Player.Instance.gameScoreRecoder.killScore.ToString();
            roundScoreText.GetComponent<TextMeshProUGUI>().text = "Round: " + Player.Instance.gameScoreRecoder.roundScore.ToString();
            goldScoreText.GetComponent<TextMeshProUGUI>().text = "Gold: " + Player.Instance.gameScoreRecoder.goldScore.ToString();
            totalScoreText.GetComponent<TextMeshProUGUI>().text = "Total: " + (Player.Instance.gameScoreRecoder.killScore + Player.Instance.gameScoreRecoder.roundScore + Player.Instance.gameScoreRecoder.goldScore).ToString();
            CalculateRank();

            backToGameStartButton.onClick.AddListener(OnBackToGameStartButtonClicked);
        }

        private void OnBackToGameStartButtonClicked()
        {
            SceneChangeManager.Instance.ChangeGameOverToGameStart();
        }

        private void CalculateRank()
        {
            int totalScore = Player.Instance.gameScoreRecoder.killScore + Player.Instance.gameScoreRecoder.roundScore + Player.Instance.gameScoreRecoder.goldScore;
            SetScoreImages();

            // Sprite[0]이 낮은 랭크 -> 갈수록 높은 랭크 이미지
            if (totalScore < 20)
            {
                RankImagePanel.GetComponent<Image>().sprite = rankImages[0];
            }
            else if (totalScore < 50)
            {
                RankImagePanel.GetComponent<Image>().sprite = rankImages[1];
            }
            else
            {
                RankImagePanel.GetComponent<Image>().sprite = rankImages[2];
            }
        }

        public void SetScoreImages()
        {
            // Set Native Size
            RankImagePanel.GetComponent<Image>().SetNativeSize();

            // ScoreImagePanel의 RectTransform을 가져와서 Anchor를 0.5, 0.5로 설정
            var rectTransform = RankImagePanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                rectTransform.anchoredPosition = new Vector2(560f, 0f);
            }
        }

        protected override void OnDeactivated()
        {
        }
    }
}