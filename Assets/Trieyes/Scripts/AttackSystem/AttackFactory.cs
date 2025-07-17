using System;
using AttackComponents;
using BattleSystem;
using CharacterSystem;
using JetBrains.Annotations;
using UnityEngine;

namespace AttackSystem
{
    using AttackID = Int32;
    
    public class AttackFactory : MonoBehaviour
    {
        public static AttackFactory Instance { get; private set; } // 싱글톤 인스턴스

        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public Attack[] attackPrefab; // 공격 프리팹 배열

        public Attack Create(AttackData attackData, Pawn attacker, [CanBeNull] Attack parent, Vector2 direction)
        {
            // attackData 변조를 막기 위한 Copy 생성
            attackData = attackData.Copy();
            
            var attack = ClonePrefab(attackData.attackId);
            
            attack.attackData = attackData;
            attack.parent = parent;

            // 유물 메인 옵션 적용
            foreach (var relic in attacker.relics)
            {
                if (relic.filterAttackTag is not null && !attack.attackData.tags.Contains(relic.filterAttackTag.Value))
                    continue;
                if (relic.filterAttackIDs is not null && !relic.filterAttackIDs.Contains(attack.attackData.attackId))
                    continue;
                
                // relic의 mainOption의 attackTag 혹은 attackID가 일치하는 상황
                foreach (var attackComponentID in relic.attackComponentIDs)
                {
                    var attackComponent = AttackComponentFactory.Instance.Create(attackComponentID, attack, direction);
                    attack.AddAttackComponent(attackComponent);
                }
            }

            // 유물 랜덤 옵션 적용
            foreach (var relic in attacker.relics)
            {
                foreach (var randomOption in relic.randomOptions)
                {
                    if (!attack.attackData.tags.Contains(randomOption.FilterTag))
                        continue;
                    
                    // RandomOption이 Attack의 Tag를 포함함
                    attack.ApplyRelicStat(randomOption.RelicStatType, randomOption.value);
                }
            }
            
            Activate(attack, attacker, parent, direction);
            return attack;
        }

        public void Activate(Attack attack, Pawn attacker, [CanBeNull] Attack parent, Vector2 direction)
        {
            if (direction.magnitude < 1e-8)
            {
                direction = Vector2.right;
            }

            attack.transform.position = parent is not null ? parent.transform.position : attacker.transform.position;
            
            var th = Mathf.Atan2(direction.y, direction.x) *  Mathf.Rad2Deg;
            attack.transform.rotation = Quaternion.Euler(new Vector3(0, 0, th));
            
            attack.attacker = attacker;
            attack.ApplyStatSheet(parent is not null ? parent.statSheet : attacker.statSheet);
            
            attack.Activate(attacker, direction.normalized);
            
            BattleStage.now.AttachAttack(attack);
            
            attack.transform.SetParent(BattleStage.now.View.transform);
            attack.gameObject.SetActive(true);
        }

        public void Deactivate(Attack attack)
        {
            attack.Deactivate();
            Destroy(attack.gameObject);
        }
        
        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// ID에 해당하는 적 프리팹을 복제하여 Pawn 컴포넌트를 반환합니다.</summary>
        /// <param name="id">적 ID</param>
        /// <returns>생성된 Pawn 컴포넌트</returns>
        private Attack ClonePrefab(AttackID id)
        {
            var attackObject = Instantiate(GetPrefabById(id));
            var attack = attackObject.GetComponent<Attack>();
            return attack;
        }

        /// <summary>
        /// ID에 해당하는 적 프리팹을 반환합니다.</summary>
        /// <param name="id">적 ID</param>
        /// <returns>해당하는 GameObject 프리팹</returns>
        private GameObject GetPrefabById(AttackID id)
        {
            return attackPrefab[id].gameObject;
            
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