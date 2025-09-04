using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;
using Utils;
using RelicSystem;
using GamePlayer;
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
        private const string ShopSceneName = "ShopScene";
        private const string GameOverSceneName = "GameOverScene";
        private int stageRound = 1;

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
        public void StartBattleSceneTest()
        {
            player = Player.Instance;
            
            LoadSceneWithCallback(BattleSceneName, OnBattleSceneLoadedWithNewCharacter);
        }

        public void StartBattleSceneTest2()
        {
            player = Player.Instance;
            
            LoadSceneWithCallback(BattleSceneName, OnBattleSceneLoadedWithNewCharacter2);
        }

        /// <summary>
        /// 전투 → 상점 씬 전환 (캐릭터 객체 전달)
        /// </summary>
        public void ChangeBattleToShop(Character mainCharacter)
        {
            BattleStageFactory.Instance.Deactivate(BattleStage.now);
            ShopSceneManager.Instance.Activate(mainCharacter, GetCurrentDifficulty());
        }

        /// <summary>
        /// 상점 → 전투 씬 전환 (캐릭터 객체 전달)
        /// </summary>
        public void ChangeShopToBattle(Character mainCharacter)
        {
            stageRound++;
            BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
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

        /// <summary>
        /// Battle Scene 전용: 새 캐릭터 생성 및 세팅 후 스테이지 생성
        /// </summary>
        private void OnBattleSceneLoadedWithNewCharacter(Scene scene)
        {
            var mainCharacter = CharacterFactory.Instance.Create(0);

            foreach (var relicId in player.selectedRelicIds)
            {
                Debug.Log($"Relic ID: {relicId}");
                mainCharacter.AddRelic(RelicFactory.Create(relicId));
            }

            mainCharacter.ApplyRelic();

            CharacterFactory.Instance.Deactivate(mainCharacter);
            BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
            ShopSceneManager.Instance.Deactivate();
        }

        private void OnBattleSceneLoadedWithNewCharacter2(Scene scene)
        {
            var mainCharacter = CharacterFactory.Instance.Create(StartSceneManager.Instance.mainCharacter.spawnID);

            mainCharacter.deck.AddCard(StartSceneManager.Instance.selectedCard.DeepCopy());
            
            mainCharacter.AddRelic(RelicFactory.Create(StartSceneManager.Instance.selectedRelic.achievementID));
            mainCharacter.ApplyRelic();

            CharacterFactory.Instance.Deactivate(mainCharacter);
            BattleStageFactory.Instance.Create(mainCharacter, GetCurrentDifficulty());
            ShopSceneManager.Instance.Deactivate();
        }
    }
}
