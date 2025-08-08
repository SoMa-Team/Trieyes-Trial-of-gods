using System;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Trieyes.Scripts.GameOver
{
    public class GameOverManager : MonoBehaviour
    {
        public Button RetryButton;

        private void Awake()
        {
            if (RetryButton != null)
            {
                RetryButton.onClick.AddListener(OnRetryButtonPressed);
            }
        }

        private void OnRetryButtonPressed()
        {
            SceneChangeManager.Instance.ChangeGameOverToGameStart();
        }
    }
}