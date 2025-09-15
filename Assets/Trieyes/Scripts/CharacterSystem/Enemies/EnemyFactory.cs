using System;
using System.Collections.Generic;
using UnityEngine;
using BattleSystem;
using Stats;
using Utils;

namespace CharacterSystem
{
    using EnemyID = Int32;
    
    /// <summary>
    /// 적 캐릭터의 생성과 관리를 담당하는 팩토리 클래스
    /// 싱글톤 패턴을 사용하여 전역적으로 접근 가능합니다.
    /// </summary>
    public class EnemyFactory : MonoBehaviour
    {
        // ===== 객체의 조회를 위한 ID =====
        private static int _enemyObjectID;
        private int getObjectID()
        {
            return _enemyObjectID++;
        }
        
        // ===== 싱글톤 =====
        public static EnemyFactory Instance { private set; get; }
        
        // ===== 적 프리팹 =====
        public GameObject[] enemyPrefabs;

        // ===== 초기화 =====
        
        /// <summary>
        /// 싱글톤 패턴을 위한 초기화
        /// 중복 인스턴스가 생성되지 않도록 합니다.
        /// </summary>
        private void Awake()
        {


            Instance = this;
        }

        // ===== 적 생성 =====
        
        /// <summary>
        /// EnemyID에 맞는 적 gameObject를 생성합니다.
        /// gameObject는 반드시 Pawn을 상속한 Unity Component가 부착되어 있습니다.
        /// </summary>
        /// <param name="id">생성할 적의 ID</param>
        /// <returns>생성된 gameObject에 부착된 Enemy 객체</returns>
        public Enemy Create(int enemyId)
        {
            var enemy = popEnemy(enemyId);
            if (enemy is null)
                enemy = ClonePrefab(enemyId);
            
            enemy.enemyID = enemyId;
            enemy.initBaseStat();
            Activate(enemy);
            
            return enemy;
        }

        // ===== 적 활성화/비활성화 =====
        
        /// <summary>
        /// 적을 활성화합니다.</summary>
        /// <param name="enemy">활성화할 적 Pawn</param>
        public void Activate(Enemy enemy)
        {
            enemy.Activate();
            enemy.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// 적을 비활성화합니다.</summary>
        /// <param name="enemy">비활성화할 적 Pawn</param>
        public void Deactivate(Enemy enemy)
        {
            enemy.Deactivate();
            enemy.gameObject.SetActive(false);
            BattleStage.now.RemoveEnemy(enemy);
            
            pushEnemy(enemy.enemyID.Value, enemy);
        }
        
        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// ID에 해당하는 적 프리팹을 복제하여 Enemy 컴포넌트를 반환합니다.</summary>
        /// <param name="id">적 ID</param>
        /// <returns>생성된 Enemy 컴포넌트</returns>
        private Enemy ClonePrefab(EnemyID id)
        {
            var enemyObject = Instantiate(GetPrefabById(id));
            var enemy = enemyObject.GetComponent<Enemy>();
            enemy.objectID = getObjectID();
            return enemy;
        }

        /// <summary>
        /// ID에 해당하는 적 프리팹을 반환합니다.</summary>
        /// <param name="id">적 ID</param>
        /// <returns>해당하는 GameObject 프리팹</returns>
        private GameObject GetPrefabById(EnemyID id)
        {
            if (!(0 <= id && id < enemyPrefabs.Length))
                throw new Exception($"Enemy (id : {id}) is not exist.");
            return enemyPrefabs[id];
        }
        
        // Object Pooling
        private Dictionary<EnemyID, Queue<Enemy>> pool = new ();

        private void pushEnemy(EnemyID id, Enemy enemy)
        {
            if (!pool.ContainsKey(id))
            {
                pool[id] = new Queue<Enemy>();
            }
            
            pool[id].Enqueue(enemy);
        }

        private Enemy popEnemy(EnemyID id)
        {
            if (!pool.ContainsKey(id))
                return null;
            if (pool[id].Count <= 0)
                return null;
            
            return pool[id].Dequeue();
        }

        public void ClearPool()
        {
            pool.Clear();
        }
    }
}