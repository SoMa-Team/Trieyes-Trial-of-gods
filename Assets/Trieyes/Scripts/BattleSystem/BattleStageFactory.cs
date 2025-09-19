using System;
using System.Collections.Generic;
using AttackSystem;
using CharacterSystem;
using UISystem;
using UnityEngine;
using Utils;
using System.Linq;
using EventType = Utils.EventType;
using GamePlayer;
using GameFramework;

namespace BattleSystem
{
    using BattleStageID = Int32;
    
    /// <summary>
    /// 전투 모드 타입을 정의하는 열거형
    /// </summary>
    public enum BattleMode
    {
        Timer,          // 타이머 기반 전투
        BreakThrough,   // 돌파형 전투
        Escape,         // 탈출형 전투
        Boss           // 보스 전투
    }
    
    /// <summary>
    /// 전투 스테이지의 생성과 관리를 담당하는 팩토리 클래스
    /// 싱글톤 패턴을 사용하여 전역적으로 접근 가능합니다.
    /// </summary>
    public class BattleStageFactory: MonoBehaviour
    {
        // ===== 스테이지 프리팹 =====
        [System.Serializable]
        public class BattleStagePrefabData
        {
            public BattleMode battleMode;
            public GameObject prefab;
        }
        
        [SerializeField] private BattleStagePrefabData[] battleStagePrefabs;
        public static BattleStageFactory Instance {private set; get;}
        [SerializeField] private OnBattleStartPopupView onBattleStartPopupView;
        
        // 현재 활성화된 BattleMode 추적
        private static BattleMode currentBattleMode;

        // ===== 초기화 =====
        
        /// <summary>
        /// 싱글톤 패턴을 위한 초기화
        /// 중복 인스턴스가 생성되지 않도록 합니다.
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ===== 스테이지 생성 =====
        
        /// <summary>
        /// 주어진 캐릭터, 난이도, 전투 모드로 새로운 전투 스테이지를 생성합니다.</summary>
        /// <param name="mainCharacter">메인 캐릭터 Pawn</param>
        /// <param name="difficulty">전투 난이도 설정</param>
        /// <param name="battleMode">전투 모드</param>
        /// <returns>생성된 BattleStage 인스턴스</returns>
        public void Create(Pawn mainCharacter, BattleMode battleMode)
        {
            // 해당 BattleMode에 맞는 프리팹 찾기
            var prefabData = Array.Find(battleStagePrefabs, data => data.battleMode == battleMode);
            if (prefabData == null)
            {
                Debug.LogError($"BattleMode {battleMode}에 해당하는 프리팹을 찾을 수 없습니다.");
                return;
            }
            
            var battleStageGameObject = Instantiate(prefabData.prefab);
            var battleStageView = battleStageGameObject.GetComponent<BattleStageView>();
            
            // BattleMode에 따라 적절한 BattleStage 인스턴스 생성
            BattleStage battleStage = CreateBattleStageInstance(battleMode);
            
            battleStageView.BattleStage = battleStage;
            
            // 현재 BattleMode 설정
            currentBattleMode = battleMode;
            
            (onBattleStartPopupView ?? OnBattleStartPopupView.Instance).Activate((Character)mainCharacter, battleStage);

            CardStatChangeRecorder.Instance.RecordStart();
            mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
            var triggerResult = CardStatChangeRecorder.Instance.RecordEnd();

            (onBattleStartPopupView ?? OnBattleStartPopupView.Instance).AnimateTriggerEvent(triggerResult);
            Player.Instance.gameScoreRecoder.roundScore += 100;
        }

        private static BattleStage CreateBattleStageInstance(BattleMode battleMode)
        {
            return battleMode switch
            {
                BattleMode.Timer => new BattleTimer(),
                BattleMode.BreakThrough => new BattleBreakThrough(),
                BattleMode.Escape => new BattleEscape(),
                BattleMode.Boss => new BattleBoss(),
                _ => new BattleTimer() // 기본값
            };
        }
        
        // ===== 현재 활성화된 BattleStage 접근 =====
        
        /// <summary>
        /// 현재 활성화된 BattleStage의 제네릭 인스턴스를 반환합니다.
        /// </summary>
        public static BattleStage GetCurrentInstance()
        {
            BattleStage currentInstance = currentBattleMode switch
            {
                BattleMode.Timer => BattleTimer.Instance,
                BattleMode.BreakThrough => BattleBreakThrough.Instance,
                BattleMode.Escape => BattleEscape.Instance,
                BattleMode.Boss => BattleBoss.Instance,
                _ => null // 기본값
            };

            if (currentInstance is null || !currentInstance.isActivated)
            {
                return null;
            }

            return currentInstance;
        }

        // ===== 스테이지 활성화/비활성화 =====
        
        /// <summary>
        /// 전투 스테이지를 활성화하고 초기 설정을 완료합니다.</summary>
        /// <param name="battleStage">활성화할 BattleStage</param>
        /// <param name="mainCharacter">메인 캐릭터 Pawn</param>
        /// <param name="difficulty">전투 난이도 설정</param>
        public void Activate(BattleStage battleStage, Pawn mainCharacter)
        {
            // 메인 캐릭터 설정
            CharacterFactory.Instance.Activate(mainCharacter);
            battleStage.mainCharacter = mainCharacter as Character;
            
            mainCharacter.transform.SetParent(battleStage.View.transform);
            
            // 카메라가 메인 캐릭터를 팔로우하도록 설정
            battleStage.View.SetMainCharacter();
            
            // 캐릭터 리스트 초기화
            battleStage.characters = new List<Pawn> { mainCharacter };
            battleStage.enemies = new();

            // 공격 초기화
            battleStage.attacks = new ();
            
            battleStage.Activate(InGameManager.Instance.GetCurrentDifficulty());
            
            // SpawnManager 초기화
            battleStage.spawnManager = SpawnManager.Instance;
            battleStage.spawnManager.Activate(battleStage.difficulty);

            BattleOverlayCanvasController.Instance.Activate();
            BattleWorldCanvasController.Instance.Activate();

            mainCharacter.OnEvent(EventType.OnBattleStart, null);
        }   

        /// <summary>
        /// 전투 스테이지를 비활성화하고 리소스를 정리합니다.</summary>
        /// <param name="battleStage">비활성화할 BattleStage</param>
        public void Deactivate(BattleStage battleStage)
        {
            Debug.Log("BattleStageFactory Deactivate Called");
            
            battleStage.mainCharacter.transform.SetParent(null);
            
            foreach (var golds in battleStage.golds.Values.ToList())
            {
                DropFactory.Instance.Deactivate(golds);
            }
            DropFactory.Instance.ClearPool();
            
            // 공격 정리
            foreach (var attack in battleStage.attacks.Values.ToList())
            {
                AttackFactory.Instance.Deactivate(attack);
            }
            AttackFactory.Instance.ClearPool();
            
            // 적 정리
            foreach (var enemy in battleStage.enemies.Values.ToList())
            {
                // TODO : Enemy가 관리되지 않는 오류
                EnemyFactory.Instance.Deactivate(enemy);
            }
            EnemyFactory.Instance.ClearPool();
            
            // 캐릭터 정리
            foreach (var character in battleStage.characters)
            {
                CharacterFactory.Instance.Deactivate(character);
            }
            
            battleStage.spawnManager.Deactivate();
            battleStage.Deactivate();

            BattleOverlayCanvasController.Instance.Deactivate();
            BattleWorldCanvasController.Instance.Deactivate();
            DamageNumberViewFactory.Instance.OnBattleEnded();
            
            Destroy(battleStage.View.gameObject);
            
            // BattleMode 초기화
            currentBattleMode = BattleMode.Timer; // 기본값으로 초기화
        }
    }
}
