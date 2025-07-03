using System;
using UnityEngine;
using CharacterSystem;
using Utils;

namespace AttackComponents
{
    using AttackComponentID = Int32;
    
    /// <summary>
    /// 플레이어 캐릭터의 생성과 관리를 담당하는 팩토리 클래스
    /// 싱글톤 패턴을 사용하여 전역적으로 접근 가능합니다.
    /// </summary>
    public class AttackComponentFactory : MonoBehaviour
    {
        // ===== 싱글톤 =====
        public static AttackComponentFactory Instance {private set; get;}

        // ===== 프리팹 =====
        public GameObject[] attackComponentPrefabs;

        // ===== 초기화 =====
        
        /// <summary>
        /// 싱글톤 패턴을 위한 초기화
        /// 중복 인스턴스가 생성되지 않도록 합니다.
        /// </summary>
        void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }

        // ===== 공격 컴포넌트 생성 =====

        /// <summary>
        /// AttackComponentID에 맞는 캐릭터 gameObject를 생성합니다.
        /// gameObject는 반드시 AttackComponent를 상속한 Unity Component가 부착되어 있습니다.
        /// </summary>
        /// <param name="id">생성할 공격 컴포넌트의 ID</param>
        /// <returns>생성된 gameObject에 부착된 AttackComponent 객체</returns>
        public AttackComponent Create(AttackComponentID id)
        {
            var attackComponent = ClonePrefab(id);
            Activate(attackComponent);
            return attackComponent;
        }
        
        // ===== 캐릭터 활성화/비활성화 =====
        
        /// <summary>
        /// AttackComponent 활성화합니다.
        /// </summary>
        /// <param name="pawn">활성화할 Attack Component</param>
        public void Activate(AttackComponent attackComponent)
        {
            attackComponent.Activate(null);
        }

        /// <summary>
        /// AttackComponent 비활성화합니다.
        /// </summary>
        /// <param name="pawn">비활성화할 Attack Component</param>
        public void Deactivate(AttackComponent attackComponent)
        {
            attackComponent.Deactivate();
        }
        
        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// ID에 해당하는 프리팹을 복제하여 AttackComponent를 반환합니다.</summary>
        /// <param name="id">공격 컴포넌트 ID</param>
        /// <returns>생성된 공격 컴포넌트</returns>
        private AttackComponent ClonePrefab(AttackComponentID id)
        {
            var attackComponentObject = Instantiate(GetPrefabById(id));
            var attackComponent = attackComponentObject.GetComponent<AttackComponent>();
            return attackComponent;
        }

        /// <summary>
        /// ID에 해당하는 AttackComponent 프리팹을 반환합니다.</summary>
        /// <param name="id">AttackComponent ID</param>
        /// <returns>해당하는 GameObject 프리팹</returns>
        private GameObject GetPrefabById(AttackComponentID id)
        {
            // TODO: characterID와 characterPrefab 매칭 필요
            return attackComponentPrefabs[id];
            
            // return id switch
            // {
            //     0 => characterPrefabs[0],
            //     1 => characterPrefabs[1],
            //     // TODO: 더 많은 캐릭터 ID 추가 필요
            //     _ => null
            // };
        }
    }
}