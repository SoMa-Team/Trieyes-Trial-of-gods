using System;
using System.Collections.Generic;
using AttackSystem;
using CharacterSystem;
using UnityEngine;
using Utils;

namespace BattleSystem
{
    using BattleStageID = Int32;
    
    /// <summary>
    /// 전투 스테이지의 생성과 관리를 담당하는 팩토리 클래스
    /// 싱글톤 패턴을 사용하여 전역적으로 접근 가능합니다.
    /// </summary>
    public class BattleStageFactory: MonoBehaviour
    {
        // ===== 스테이지 프리팹 =====
        public GameObject[] battleStagePrefabs;
        public static BattleStageFactory Instance {private set; get;}

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

        // ===== 스테이지 생성 =====
        
        /// <summary>
        /// 주어진 캐릭터와 난이도로 새로운 전투 스테이지를 생성합니다.</summary>
        /// <param name="mainCharacter">메인 캐릭터 Pawn</param>
        /// <param name="difficulty">전투 난이도 설정</param>
        /// <returns>생성된 BattleStage 인스턴스</returns>
        public BattleStage Create(Pawn mainCharacter, Difficulty difficulty)
        {
            BattleStageID battleStageID = 0; // TODO: 난이도와 연동하여 스테이지 ID 설정 필요
            
            var battleStageGameObject = Instantiate(battleStagePrefabs[battleStageID]);
            var battleStageView = battleStageGameObject.GetComponent<BattleStageView>();
            var battleStage = new BattleStage();
            battleStageView.BattleStage = battleStage;

            Activate(battleStage, mainCharacter, difficulty);
            return battleStage;
        }

        // ===== 스테이지 활성화/비활성화 =====
        
        /// <summary>
        /// 전투 스테이지를 활성화하고 초기 설정을 완료합니다.</summary>
        /// <param name="battleStage">활성화할 BattleStage</param>
        /// <param name="mainCharacter">메인 캐릭터 Pawn</param>
        /// <param name="difficulty">전투 난이도 설정</param>
        public void Activate(BattleStage battleStage, Pawn mainCharacter, Difficulty difficulty)
        {
            // 메인 캐릭터 설정
            CharacterFactory.Instance.Activate(mainCharacter);
            battleStage.mainCharacter = mainCharacter;
            
            mainCharacter.transform.SetParent(battleStage.View.transform);
            
            // 카메라가 메인 캐릭터를 팔로우하도록 설정
            battleStage.View.SetMainCharacter();
            
            // 캐릭터 리스트 초기화
            battleStage.characters = new List<Pawn>();
            battleStage.characters.Add(mainCharacter);
            battleStage.enemies = new();
            
            // 기타 설정
            battleStage.difficulty = difficulty;
            battleStage.attacks = new ();
            
            // SpawnManager 초기화
            battleStage.spawnManager = SpawnManager.Instance;
            battleStage.spawnManager.Activate(difficulty);
            
            battleStage.Activate();
        }

        /// <summary>
        /// 전투 스테이지를 비활성화하고 리소스를 정리합니다.</summary>
        /// <param name="battleStage">비활성화할 BattleStage</param>
        public void Deactivate(BattleStage battleStage)
        {
            // 캐릭터 정리
            foreach (var character in battleStage.characters)
            {
                CharacterFactory.Instance.Deactivate(character);
            }

            // 적 정리
            foreach (var enemy in new List<Pawn>(battleStage.enemies.Values))
            {
                EnemyFactory.Instance.Deactivate(enemy);
            }
            
            // 공격 정리
            foreach (var attack in new List<Attack>(battleStage.attacks.Values))
            {
                AttackFactory.Instance.Deactivate(attack);
            }
            battleStage.Deactivate();
            // SpawnManager 비활성화
            battleStage.spawnManager.Deactivate();
        }
    }
}
