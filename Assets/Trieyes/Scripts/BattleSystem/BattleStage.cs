using System;
using CharacterSystem;
using System.Collections.Generic;
using AttackSystem;
using GameFramework;
using NodeStage;
using UnityEngine;
using System.Linq;

namespace BattleSystem
{
    /// <summary>
    /// 전투 스테이지의 핵심 데이터와 상태를 관리하는 추상 클래스
    /// 현재 활성화된 전투 스테이지의 정보를 담고 있습니다.
    /// </summary>
    public abstract class BattleStage //TODO: 안정화 시 NodeStage 시스템과 통합 요망
    {
        // ===== 전역 스테이지 관리 =====
        public static BattleStage now => BattleStageFactory.GetCurrentInstance();
        public BattleStageView View { set; get; }
        public float elapsedTime => Time.time - startTime;

        // ===== 전투 스테이지 데이터 =====
        public Difficulty difficulty;
        public Character mainCharacter;
        public List<Pawn> characters = new ();
        public Dictionary<int, Enemy> enemies = new ();

        private Dictionary<int, Enemy> liveEnemies => enemies.Where(e => e.Value as Enemy is not null && !e.Value.isDead).ToDictionary(e => e.Key, e => e.Value as Enemy);
        public Dictionary<int, Attack> attacks = new ();
        public Dictionary<int, Gold> golds = new ();
        public SpawnManager spawnManager;

        public List<BattleSubsystem> subsystems = new List<BattleSubsystem>();
        
        public bool isActivated = false;

        // ===== 시간 관리 관련 =====
        protected float startTime;

        protected float ticDuration = 0.5f;
        protected float lastTick = -1;
        public abstract void Update();

        // ===== 스테이지 활성화, 비활성화 =====

        /// <summary>
        /// 전투 스테이지를 활성화합니다.
        /// 동시에 하나의 스테이지만 활성화될 수 있습니다.
        /// </summary>
        public virtual void Activate(Difficulty difficulty)
        {
            if (now is not null)
            {
                throw new Exception("There must be exactly one BattleStage.");
            }
            
            this.difficulty = difficulty;
            startTime = Time.time;
            lastTick = Time.time;
            isActivated = true;
            View.gameObject.SetActive(true);
            
            OnActivated();
        }

        /// <summary>
        /// 전투 스테이지를 비활성화합니다.</summary>
        public virtual void Deactivate()
        {
            OnDeactivated();
            
            Debug.Log("Deactivating battle stage.");

            foreach (var subsystem in subsystems)
            {
                subsystem.Deactivate();
            }
            subsystems.Clear();

            isActivated = false;
            difficulty = null; // difficulty를 null로 설정하여 Update에서 오류 방지
        }

        // 파생 클래스에서 구현할 추상 메서드들
        protected virtual void OnActivated() { }
        protected virtual void OnDeactivated() { }

        // ===== 적 관리 =====
        
        /// <summary>
        /// 생성된 적을 스테이지에 연결하고 위치를 설정합니다.
        /// 적을 enemies 리스트에 추가하여 관리합니다.
        /// </summary>
        /// <param name="enemy">연결할 적 Pawn</param>
        /// <param name="spawnPoint">스폰 포인트 Transform</param>
        public void AttachEnemy(Enemy enemy, Vector3 spawnPosition)
        {
            enemy.transform.SetParent(View.transform);
            enemy.transform.position = spawnPosition;
            enemies.Add(enemy.objectID, enemy);
        }
        
        public void RemoveEnemy(Enemy enemy)
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

        public void AttachGold(Gold gold)
        {
            golds.Add(gold.objectID, gold);
        }

        public void RemoveGold(Gold gold)
        {
            golds.Remove(gold.objectID);
        }

        // 전투 클리어 시 호출
        public virtual void OnBattleClear()
        {
            BattleStageFactory.Instance.Deactivate(this);
            InGameManager.Instance.StartNextStage(StageType.BattleReward, mainCharacter);
        }
        
        // 플레이어 사망 시 호출
        public void OnPlayerDeath()
        {
            BattleStageFactory.Instance.Deactivate(this);
            InGameManager.Instance.StartNextStage(StageType.GameOver, mainCharacter);
        }

        public float GetTime()
        {
            return Time.time - startTime;
        }

        public void AddTime(float time)
        {
            difficulty.battleLength += time;
        }

        public List<Enemy> GetEnemiesInRectRange(Vector2 start, Vector2 end)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                // 파괴된 객체 체크
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (enemy.Value.transform.position.x > start.x && enemy.Value.transform.position.x < end.x &&
                    enemy.Value.transform.position.y > start.y && enemy.Value.transform.position.y < end.y)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInRectRangeFromTarget(Pawn target, Vector2 start, Vector2 end)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (enemy.Value.transform.position.x > start.x && enemy.Value.transform.position.x < end.x &&
                    enemy.Value.transform.position.y > start.y && enemy.Value.transform.position.y < end.y)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInRectRangeFromTargetOrderByDistance(Pawn target, Vector2 start, Vector2 end)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (enemy.Value.transform.position.x > start.x && enemy.Value.transform.position.x < end.x &&
                    enemy.Value.transform.position.y > start.y && enemy.Value.transform.position.y < end.y)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            enemiesInRange.Sort((a, b) => Vector2.Distance(a.transform.position, target.transform.position).CompareTo(Vector2.Distance(b.transform.position, target.transform.position)));
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInCircleRangeFromTargetOrderByDistance(Pawn target, float radius)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (Vector2.Distance(enemy.Value.transform.position, target.transform.position) < radius)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            enemiesInRange.Sort((a, b) => Vector2.Distance(a.transform.position, target.transform.position).CompareTo(Vector2.Distance(b.transform.position, target.transform.position)));
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInRectRangeOrderByDistance(Vector2 start, Vector2 end)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                // 파괴된 객체 체크
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (enemy.Value.transform.position.x > start.x && enemy.Value.transform.position.x < end.x &&
                    enemy.Value.transform.position.y > start.y && enemy.Value.transform.position.y < end.y)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            enemiesInRange.Sort((a, b) => Vector2.Distance(a.transform.position, start).CompareTo(Vector2.Distance(b.transform.position, start)));
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInCircleRange(Vector2 start, float radius)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                // 파괴된 객체 체크
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (Vector2.Distance(enemy.Value.transform.position, start) < radius)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            return enemiesInRange;
        }

        public List<Enemy> GetEnemiesInCircleRangeOrderByDistance(Vector2 start, float radius, int count)
        {
            List<Enemy> enemiesInRange = new List<Enemy>();
            foreach (var enemy in liveEnemies)
            {
                // 파괴된 객체 체크
                if (enemy.Value is null || enemy.Value.transform == null)
                {
                    continue;
                }
                
                if (Vector2.Distance(enemy.Value.transform.position, start) < radius)
                {
                    enemiesInRange.Add(enemy.Value as Enemy);
                }
            }
            enemiesInRange.Sort((a, b) => Vector2.Distance(a.transform.position, start).CompareTo(Vector2.Distance(b.transform.position, start)));
            return enemiesInRange;
        }
    }

    public abstract class BattleStage<T> : BattleStage where T : BattleStage<T>
    {
        public static T Instance { get; private set; }

        protected BattleStage()
        {
            Instance = (T)this;
        }
    }
} 
