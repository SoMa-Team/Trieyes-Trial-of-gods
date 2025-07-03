using System;
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
            var attack = ClonePrefab(attackData.attackId);
            attack.attackData = attackData;
            attack.parent = parent;
            Activate(attack, attacker, parent, direction);
            return attack;
        }

        public void Activate(Attack attack, Pawn attacker, [CanBeNull] Attack parent, Vector2 direction)
        {
            if (direction.magnitude < 1e-8)
            {
                direction = Vector2.right;
            }

            if (parent is not null)
            {
                attack.transform.position = parent.transform.position;    
            }
            else
            {
                attack.transform.position = attacker.transform.position;
            }
            
            var th = Mathf.Atan2(direction.y, direction.x) *  Mathf.Rad2Deg;
            attack.transform.rotation = Quaternion.Euler(new Vector3(0, 0, th));
            
            attack.Activate(attacker, direction.normalized);
            
            attack.transform.SetParent(BattleStage.now.View.transform);
            BattleStage.now.AttachAttack(attack);
        }

        public void Deactivate(Attack attack)
        {
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