using System;
using CharacterSystem;
using System.Collections.Generic;
using AttackSystem;
using JetBrains.Annotations;
using UnityEngine;
using Utils;

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
        public List<Pawn> characters = new List<Pawn>();
        public List<Pawn> enemies = new List<Pawn>();
        public List<Attack> attacks = new List<Attack>();
        public SpawnManager spawnManager;
        
        private float time;

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
            
            now = this;
            time = 0.0f;
        }

        /// <summary>
        /// 전투 스테이지를 비활성화합니다.</summary>
        public void Deactivate()
        {
            now = null;
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
            enemies.Add(enemy);
        }

        public float GetTime()
        {
            // TODO: 시간을 계산하는 기능 필요
            return time;
        }
    }
} 