using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;
using Utils;
using RelicSystem;
using Unity.VisualScripting;
using GamePlayer;

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

        /// <summary>
        /// 전투 → 상점 씬 전환 (캐릭터 객체 전달)
        /// </summary>
        public void ChangeBattleToShop(Character mainCharacter)
        {
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
    }
    // 720004,RAC002,방패 밀쳐내기의 범위가 줄어들고 넉백이 사라집니다. 쿨타임이 [#]초로 감소합니다. 기본공격을 더 이상 사용할 수 없습니다,10002,10001,
    // 720005,RAC003,방패 돌진이 방어력에 비례하는 추가 피해를 입힙니다,10003,10002,
    // 720006,RAC004,전장의 포효가 사용자의 체력을 일부 회복시킵니다,10004,10003,
    // 720007,RAC005,속성 부여가 무작위 속성을 부여하는 대신 천상 속성을 부여합니다,10005,10004,
    // 720008,RAC006,속성이 부여되면 공격할 때마다 전방으로 해당 속성의 검기를 발사합니다,10006,1001,
    // 720009,RAC007,속성이 부여되는 동안 함께 전투를 도와주는 해당 속성의 정령이 소환됩니다,10007,10006,
    // 720010,RAC008,속성 부여 상태에서 적을 죽일 때마다 지속시간이 0.1초 증가합니다,10008,1001,
    // 720011,RAC009,속성 부여 스킬을 사용할 때마다 자신의 주위를 공전하는 별을 생성합니다. 별에 닿을 때마다 적은 피해를 받습니다,10009,1001,
    // 720012,RAC010,속성 부여가 번개 속성로 고정됩니다. 번개 속성이 부여됐을 때 공격속도가 크게 증가합니다,10010,1001,
    // 720013,RAC011,속성 부여가 불 속성으로 고정됩니다. 화상을 입은 적이 다시 화상을 입는 경우 남은 화상 피해량의 20퍼센트를 즉시 입으며 화상의 지속시간이 초기화됩니다.,10011,1001,
    // 720014,RAC012,속성부여가 얼음 속성으로 고정됩니다. 둔화가 걸린 적이 다시 둔화에 걸리는 경우 해당 적의 방어력이 감소합니다.,10012,1001,
}
