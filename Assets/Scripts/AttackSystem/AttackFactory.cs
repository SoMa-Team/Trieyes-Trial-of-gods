using System;
using BattleSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackSystem
{
    using AttackID = Int32;
    
    public class AttackFactory : MonoBehaviour
    {
        public static AttackFactory Instance { get; private set; }

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
        
        public Attack[] attackPrefab;

        public Attack Create(AttackData attackData, Pawn attacker)
        {
            var attack = ClonePrefab(attackData.attackId);
            Activate(attack, attacker);
            return attack;
        }

        public void Activate(Attack attack, Pawn attacker)
        {
            attack.Activate(attacker);

            BattleStage.now.AttachAttack(attack);
        }

        public void Deactivate(Attack attack)
        {
            attack.Deactivate();
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
            // TODO: AttackID와 prefab 매칭 필요
            return attackPrefab[0].gameObject;
            
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