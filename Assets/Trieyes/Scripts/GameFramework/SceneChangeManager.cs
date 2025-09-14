using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;
using Utils;
using RelicSystem;
using GamePlayer;
using NodeStage;
using OutGame;
using Unity.VisualScripting;
using GameOver;

namespace GameFramework
{
    /// <summary>
    /// 씬 전환 및 캐릭터 전달을 관리하는 싱글턴 매니저
    /// </summary>
    public class SceneChangeManager : MonoBehaviour
    {
        // ====== 싱글턴 ======
        public static SceneChangeManager Instance { get; private set; }

        // ====== 씬 이름 상수 ======
        private const string GameStartSceneName = "GameStart";
        private const string BattleSceneName = "BattleScene";
        private const string GameOverSceneName = "GameOverScene";

        // ====== 초기화 ======
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        // ====== 씬 전환 기능 ======

        /// <summary>
        /// 전투 테스트 씬 시작 (캐릭터 자동 생성)
        /// </summary>
        public void GameStart()
        {
            LoadSceneWithCallback(BattleSceneName, OnBattleSceneLoadedWithNewCharacter);
        }

        public void ChangeBattleToGameOver()
        {
            LoadSceneWithCallback(GameOverSceneName, scene =>
            {
                
            });
        }

        public void ChangeGameOverToGameStart()
        {
            GameOverManager.Instance.Deactivate();
            
            LoadSceneWithCallback(GameStartSceneName, scene =>
            {
                OnGameOverSceneLoaded(scene);
            });
        }

        // ====== 헬퍼 메서드 ======

        /// <summary>
        /// 공통: 씬 로드 후 콜백 처리
        /// </summary>
        private void LoadSceneWithCallback(string sceneName, Action<Scene> onLoaded)
        {
            void Handler(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= Handler;
                onLoaded?.Invoke(scene);
            }
            SceneManager.sceneLoaded += Handler;
            SceneManager.LoadScene(sceneName);
        }

        private void OnBattleSceneLoadedWithNewCharacter(Scene scene)
        {
            var mainCharacter = CharacterFactory.Instance.Create(Player.Instance.mainCharacterId);

            CharacterFactory.Instance.Deactivate(mainCharacter);
            NextStageSelectPopup.Instance.SetNextStage(null, (Character)mainCharacter, true);
        }

        private void OnGameOverSceneLoaded(Scene scene)
        {
            Player.Instance.Deactivate();
        }
    }
}
