using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;
using Utils;
using RelicSystem;

namespace GameFramework
{
    public class TSceneChangeManager : MonoBehaviour
    {
        // ===== [기능 1] 싱글턴 및 데이터 전달 =====
        public static TSceneChangeManager Instance {private set; get;}
        
        private readonly string BATTLE_SCENE_NAME = "SampleBattleScene";
        private readonly string SHOP_SCENE_NAME = "SampleShopScene";
        private int stageRound = 12;

        // ===== 초기화 =====
        /// <summary>
        /// 싱글톤 패턴을 위한 초기화
        /// 중복 인스턴스가 생성되지 않도록 합니다.
        /// </summary>
        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(this);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        private void OnDestroy()
        {
        }

        // ===== [기능 2] 씬 전환 =====
        public void StartBattleSceneTest()
        {
            //DontDestroyOnLoad(mainCharacter.gameObject);
            
            void OnStartBattleSceneTest(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= OnStartBattleSceneTest;
                var mainCharacter = CharacterFactory.Instance.Create(0);
                mainCharacter.AddRelic(RelicFactory.Create(720001));
                mainCharacter.AddRelic(RelicFactory.Create(720002));
                mainCharacter.ApplyRelic();
        
                CharacterFactory.Instance.Deactivate(mainCharacter);
                Difficulty difficulty = Difficulty.GetByStageRound(stageRound);
                BattleStageFactory.Instance.Create(mainCharacter, difficulty);
            }
            
            SceneManager.sceneLoaded += OnStartBattleSceneTest;
            SceneManager.LoadScene(BATTLE_SCENE_NAME);
        }

        public void ChangeBattleToShop(Character mainCharacter)
        {
            mainCharacter.transform.SetParent(null);
            DontDestroyOnLoad(mainCharacter.gameObject);

            void OnChangeBattleToShop(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= OnChangeBattleToShop;
                ShopSceneManager.Instance.Activate(mainCharacter, Difficulty.GetByStageRound(stageRound));
            }
            SceneManager.sceneLoaded += OnChangeBattleToShop;
            SceneManager.LoadScene(SHOP_SCENE_NAME);
        }
    }
} 