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

        public Player player;

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
            player = Player.Instance;
            
            LoadSceneWithCallback(BattleSceneName, GameStartWithNewCharacter);
        }

        public void ChangeBattleToGameOver()
        {
            LoadSceneWithCallback(GameOverSceneName, scene =>
            {
                
            });
        }

        public void ChangeGameOverToGameStart()
        {
            LoadSceneWithCallback(GameStartSceneName, scene =>
            {
            
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

        /// <summary>
        /// 공통: 캐릭터 DontDestroyOnLoad 및 부모 분리
        /// </summary>
        private void PrepareCharacterForSceneTransition(Character character)
        {
            if (character == null) return;
            character.transform.SetParent(null);
            DontDestroyOnLoad(character.gameObject);
        }

        /// <summary>
        /// 현재 스테이지 난이도 반환
        /// </summary>
        private Difficulty GetCurrentDifficulty()
        {
            return Difficulty.GetByStageRound(stageRound);
        }

        private void OnBattleSceneLoadedWithNewCharacter(Scene scene)
        {
            var mainCharacter = CharacterFactory.Instance.Create(Player.Instance.mainCharacterId);

            if (Player.Instance.selectedCard is not null)
            {
                mainCharacter.deck.AddCard(Player.Instance.selectedCard.DeepCopy());
            }

            if (Player.Instance.selectedRelic is not null)
            {
                mainCharacter.AddRelic(RelicFactory.Create(Player.Instance.selectedRelic.achievementID));
                mainCharacter.ApplyRelic();
            }

            CharacterFactory.Instance.Deactivate(mainCharacter);
            NextStageSelectPopup.Instance.StartGame((Character)mainCharacter);
            ShopSceneManager.Instance.Deactivate();
        }
    }
}
