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
        public List<Pawn> characters = new ();
        public List<Pawn> enemies = new ();
        public List<Attack> attacks = new ();
        public SpawnManager spawnManager;

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

            foreach (var enemy in enemies)
            {
                EnemyFactory.Instance.Deactivate(enemy);
            }
            
            foreach (var attack in attacks)
            {
                AttackFactory.Instance.Deactivate(attack);
            }
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

        public void AttachAttack(Attack attack)
        {
            attacks.Add(attack);
        }
        
        // ===== 시간 관리 관련 =====
        private float startTime;

        public float GetTime()
        {
            return Time.time - startTime;
        }

        public List<Enemy> GetEnemiesInRectRange(Vector2 start, Vector2 end)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                // 파괴된 객체 체크
                if (enemy == null || enemy.transform == null)
                {
                    continue;
                }
                
                if (enemy.transform.position.x > start.x && enemy.transform.position.x < end.x &&
                    enemy.transform.position.y > start.y && enemy.transform.position.y < end.y)
                {
                    enemiesInRange.Add(enemy as Enemy);
                }
            }
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInRectRangeOrderByDistance(Vector2 start, Vector2 end)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                // 파괴된 객체 체크
                if (enemy == null || enemy.transform == null)
                {
                    continue;
                }
                
                if (enemy.transform.position.x > start.x && enemy.transform.position.x < end.x &&
                    enemy.transform.position.y > start.y && enemy.transform.position.y < end.y)
                {
                    enemiesInRange.Add(enemy as Enemy);
                }
            }
            enemiesInRange.Sort((a, b) => Vector2.Distance(a.transform.position, start).CompareTo(Vector2.Distance(b.transform.position, start)));
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInCircleRange(Vector2 start, float radius)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                // 파괴된 객체 체크
                if (enemy == null || enemy.transform == null)
                {
                    continue;
                }
                
                if (Vector2.Distance(enemy.transform.position, start) < radius)
                {
                    enemiesInRange.Add(enemy as Enemy);
                }
            }
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInCircleRangeOrderByDistance(Vector2 start, float radius, int count)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in enemies)
            {
                // 파괴된 객체 체크
                if (enemy == null || enemy.transform == null)
                {
                    continue;
                }
                
                if (Vector2.Distance(enemy.transform.position, start) < radius)
                {
                    enemiesInRange.Add(enemy as Enemy);
                }
            }
            enemiesInRange.Sort((a, b) => Vector2.Distance(a.transform.position, start).CompareTo(Vector2.Distance(b.transform.position, start)));
            return enemiesInRange;
        }

    }
} 