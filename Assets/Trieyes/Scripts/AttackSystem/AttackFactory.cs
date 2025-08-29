using System;
using System.Collections.Generic;
using System.Linq;
using AttackComponents;
using BattleSystem;
using CharacterSystem;
using JetBrains.Annotations;
using UnityEngine;

namespace AttackSystem
{
    using AttackID = Int32;

    [Serializable]
    public class IDAttackPair
    {
        public AttackID id;
        public Attack attackPrefab;
    }
    
    public class AttackFactory : MonoBehaviour
    {
        private static int _attackObjectID = 0; 
        public static AttackFactory Instance { get; private set; } // 싱글톤 인스턴스

        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitAttackPrefab();
            DontDestroyOnLoad(gameObject);
        }

        private void InitAttackPrefab()
        {
            foreach (var attackPair in rawAttackPrefab)
            {
                attackPrefab[attackPair.id] = attackPair.attackPrefab;
            }
        }

        public List<IDAttackPair> rawAttackPrefab;
        private Dictionary<AttackID, Attack> attackPrefab = new (); // 공격 프리팹 배열
        
        // ===== 공격 등록 기능 =====
        public HashSet<AttackID> registeredAttackIDs = new();
        public AttackData RegisterRelicAppliedAttack(AttackData attackData, Pawn owner)
        {
            // attackData 변조를 막기 위한 Copy 생성
            attackData = attackData.Copy();
            
            var attack = ClonePrefab(attackData.attackId);

            // 유물 메인 옵션 적용
            foreach (var relic in owner.relics)
            {
                if (relic.filterAttackTag is not null && attackData.tags is not null && !attackData.tags.Contains(relic.filterAttackTag.Value))
                    continue;
                if (relic.filterAttackIDs is not null && !relic.filterAttackIDs.Contains(attackData.attackId))
                    continue;
                
                // relic의 mainOption의 attackTag 혹은 attackID가 일치하는 상황
                foreach (var attackComponentID in relic.attackComponentIDs)
                {
                    var attackComponent = AttackComponentFactory.Instance.Create(attackComponentID, relic.level, attack, Vector2.zero);
                    attack.AddAttackComponent(attackComponent);
                }
            }

            // 유물 랜덤 옵션 적용
            foreach (var relic in owner.relics)
            {
                foreach (var randomOption in relic.randomOptions)
                {
                    if (attackData.tags is null || !attackData.tags.Contains(randomOption.FilterTag))
                        continue;
                    
                    // RandomOption이 Attack의 Tag를 포함함
                    attack.ApplyRelicStat(randomOption.RelicStatType, randomOption.value);
                }
            }

            var id = RegisterAttack(attack);
            var newAttackData = attackData.Copy();
            newAttackData.attackId = id;
            return newAttackData;
        }

        private AttackID RegisterAttack(Attack attack)
        {
            var id = attackPrefab.Keys.Max() + 1;
            registeredAttackIDs.Add(id);
            attackPrefab[id] = attack;
            return id;
        }

        public void DeregisterAttack(AttackData basicAttack)
        {
            var attackID = basicAttack.attackId;
            attackPrefab.Remove(attackID);
        }
        
        // ===== 공격 생성 =====
        public Attack Create(AttackData attackData, Pawn attacker, [CanBeNull] Attack parent, Vector2 direction)
        {
            // attackData 변조를 막기 위한 Copy 생성
            attackData = attackData.Copy();

            Attack attack = popAttack(attackData.attackId);
            if (attack is null)
                attack = ClonePrefab(attackData.attackId);
            attack.attackData = attackData;
            Activate(attack, attacker, parent, direction);
            return attack;
        }

        public void Activate(Attack attack, Pawn attacker, [CanBeNull] Attack parent, Vector2 direction)
        {
            if (direction.magnitude < 1e-8)
            {
                direction = Vector2.right;
            }
            
            attack.parent = parent;
            
            attack.transform.position = parent is not null ? parent.transform.position : attacker.transform.position;
            var th = Mathf.Atan2(direction.y, direction.x) *  Mathf.Rad2Deg;
            attack.transform.rotation = Quaternion.Euler(new Vector3(0, 0, th));
            
            attack.attacker = attacker;
            attack.ApplyStatSheet(parent is not null ? parent.statSheet : attacker.statSheet);
            
            attack.Activate(attacker, direction.normalized);
            
            BattleStage.now.AttachAttack(attack);
            
            attack.transform.SetParent(BattleStage.now.View.transform);
            
            try
            {
                attack.gameObject.SetActive(true);
            }
            catch(Exception e){
                Debug.LogError(e);
            }
        }

        public void Deactivate(Attack attack)
        {
            if(attack == null) Debug.Log("attack is null");
            if(BattleStage.now == null) Debug.Log("now is null");
            attack.Deactivate();
            attack.gameObject.SetActive(false);
            BattleStage.now.RemoveAttack(attack);
            pushAttack(attack);
        }

        public void ClearPool()
        {
            pool.Clear();
        }

        // ===== 오브젝트 풀링 =====
        private Dictionary<AttackID, Queue<Attack>> pool = new ();
        private void pushAttack(Attack attack)
        {
            var id = attack.attackData.attackId;
            if (!pool.ContainsKey(id))
                pool[id] = new Queue<Attack>();
            pool[id].Enqueue(attack);
        }
        
        private Attack popAttack(int id)
        {
            if (!pool.ContainsKey(id) || pool[id].Count == 0)
                return null;

            var attack = pool[id].Dequeue();
            var originalAttack = GetPrefabById(id);
            foreach (var key in originalAttack.relicStats.Keys)
                attack.relicStats[key] = originalAttack.relicStats[key];
            return attack;
        }
        
        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// ID에 해당하는 적 프리팹을 복제하여 Pawn 컴포넌트를 반환합니다.</summary>
        /// <param name="id">적 ID</param>
        /// <returns>생성된 Pawn 컴포넌트</returns>
        private Attack ClonePrefab(AttackID id)
        {
            var originalAttack = GetPrefabById(id);
            var attackObject = Instantiate(originalAttack.gameObject);
            var attack = attackObject.GetComponent<Attack>();

            foreach (var key in originalAttack.relicStats.Keys)
            {
                attack.relicStats[key] = originalAttack.relicStats[key];
            }

            attack.objectID = GetObjectID();
            return attack;
        }

        private int GetObjectID()
        {
            return _attackObjectID++;
        }

        /// <summary>
        /// ID에 해당하는 적 프리팹을 반환합니다.</summary>
        /// <param name="id">적 ID</param>
        /// <returns>해당하는 GameObject 프리팹</returns>
        private Attack GetPrefabById(AttackID id)
        {
            if (!attackPrefab.ContainsKey(id))
                throw new Exception($"Attack (id : {id}) is not exist.");
            return attackPrefab[id];
            
            // return id switch
            // {
            //     0 => attackPrefab[0],
            //     1 => attackPrefab[1],
            //     // TODO: 더 많은 공격 ID 추가 필요
            //     _ => null
            // };
        }
    }
}