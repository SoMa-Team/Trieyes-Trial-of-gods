using System;
using CharacterSystem;
using System.Collections.Generic;
using AttackSystem;
using JetBrains.Annotations;
using Stats;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Utils;
using EventType = UnityEngine.EventType;

namespace BattleSystem
{
    /// <summary>
    /// 전투 스테이지의 핵심 데이터와 상태를 관리하는 클래스
    /// 현재 활성화된 전투 스테이지의 정보를 담고 있습니다.
    /// </summary>
    public class BattleStage
    {
        // ===== 전역 스테이지 관리 =====
        public static BattleStage now;
        public BattleStageView View { set; get; }

        // ===== 전투 스테이지 데이터 =====
        public Difficulty difficulty;
        public Pawn mainCharacter;
        public List<Pawn> characters = new ();
        public Dictionary<int, Pawn> enemies = new ();
        public Dictionary<int, Attack> attacks = new ();
        public SpawnManager spawnManager;

        public void Update()
        {
            if (Time.time - startTime >= difficulty.battleLength)
            {
                OnBattleClear();
            }
        }

        // ===== 스테이지 활성화, 비활성화 =====

        /// <summary>
        /// 전투 스테이지를 활성화합니다.
        /// 동시에 하나의 스테이지만 활성화될 수 있습니다.
        /// </summary>
        public void Activate()
        {
            if (now is not null)
            {
                throw new Exception("There must be exactly one BattleStage.");
            }
            
            startTime = Time.time;
            now = this;
            View.gameObject.SetActive(true);
        }

        /// <summary>
        /// 전투 스테이지를 비활성화합니다.</summary>
        public void Deactivate()
        {
            now = null;
            View.gameObject.SetActive(false);
        }

        // ===== 적 관리 =====
        
        /// <summary>
        /// 생성된 적을 스테이지에 연결하고 위치를 설정합니다.
        /// 적을 enemies 리스트에 추가하여 관리합니다.
        /// </summary>
        /// <param name="enemy">연결할 적 Pawn</param>
        /// <param name="spawnPoint">스폰 포인트 Transform</param>
        public void AttachEnemy(Pawn enemy, Transform spawnPoint)
        {
            enemy.transform.SetParent(View.transform);
            enemy.transform.position = spawnPoint.position;
            enemies.Add(enemy.objectID, enemy);
        }
        
        public void RemoveEnemy(Pawn enemy)
        {
            enemies.Remove(enemy.objectID);
        }

        public void AttachAttack(Attack attack)
        {
            attacks.Add(attack.objectID, attack);
        }

        public void RemoveAttack(Attack attack)
        {
            attacks.Remove(attack.objectID);
        }

        // 전투 클리어 시 호출
        public void OnBattleClear()
        {
            Debug.LogError("OnBattleClear");
            BattleStageFactory.Instance.Deactivate(this);
            // Todo: SceneChangeManager 호출
        }
        
        // 플레이어 사망 시 호출
        public void OnPlayerDeath()
        {
            // TODO : Character 클래스 분리 후 구현
        }
        
        // ===== 시간 관리 관련 =====
        private float startTime;

        public float GetTime()
        {
            return Time.time - startTime;
        }
    }
} 