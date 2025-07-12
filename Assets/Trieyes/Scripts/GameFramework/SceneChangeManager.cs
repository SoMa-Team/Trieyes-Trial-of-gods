using System;
using BattleSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using CharacterSystem;
using Utils;

namespace GameFramework
{
    public class SceneChangeManager : MonoBehaviour
    {
        // ===== [기능 1] 싱글턴 및 데이터 전달 =====
        public static SceneChangeManager Instance {private set; get;}
        
        private readonly string BATTLE_SCENE_NAME = "BattleSceneTest";

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
        public void StartBattleSceneTest(Pawn mainCharacter)
        {
            DontDestroyOnLoad(mainCharacter.gameObject);
            
            void OnStartBattleSceneTest(Scene scene, LoadSceneMode mode)
            {
                SceneManager.sceneLoaded -= OnStartBattleSceneTest;
                BattleStageFactory.Instance.Create(mainCharacter, Difficulty.GetByStageRound(1));
            }
            
            SceneManager.sceneLoaded += OnStartBattleSceneTest;
            SceneManager.LoadScene(BATTLE_SCENE_NAME);
        }
    }
} 