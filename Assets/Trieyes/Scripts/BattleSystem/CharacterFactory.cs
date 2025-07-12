using System;
using UnityEngine;
using CharacterSystem;
using Utils;

namespace BattleSystem
{
    using CharacterID = Int32;
    
    /// <summary>
    /// 플레이어 캐릭터의 생성과 관리를 담당하는 팩토리 클래스
    /// 싱글톤 패턴을 사용하여 전역적으로 접근 가능합니다.
    /// </summary>
    public class CharacterFactory : MonoBehaviour
    {
        // ===== 싱글톤 =====
        public static CharacterFactory Instance {private set; get;}

        // ===== 캐릭터 프리팹 =====
        public GameObject[] characterPrefabs;

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

        // ===== 캐릭터 생성 =====

        /// <summary>
        /// CharacterID에 맞는 캐릭터 gameObject를 생성합니다.
        /// gameObject는 반드시 Pawn을 상속한 Unity Component가 부착되어 있습니다.
        /// </summary>
        /// <param name="id">생성할 캐릭터의 ID</param>
        /// <returns>생성된 gameObject에 부착된 Pawn 객체</returns>
        public Pawn Create(CharacterID id)
        {
            var pawn = ClonePrefab(id);
            Activate(pawn);
            return pawn;
        }
        
        // ===== 캐릭터 활성화/비활성화 =====
        
        /// <summary>
        /// 캐릭터를 활성화합니다.
        /// Pawn의 Activate 메서드를 호출합니다.
        /// </summary>
        /// <param name="pawn">활성화할 캐릭터 Pawn</param>
        public void Activate(Pawn pawn)
        {
            pawn.Activate();
        }

        /// <summary>
        /// 캐릭터를 비활성화합니다.
        /// Pawn의 Deactivate 메서드를 호출합니다.
        /// </summary>
        /// <param name="pawn">비활성화할 캐릭터 Pawn</param>
        public void Deactivate(Pawn pawn)
        {
            pawn.Deactivate();
        }
        
        // ===== 내부 헬퍼 =====
        
        /// <summary>
        /// ID에 해당하는 프리팹을 복제하여 Pawn 컴포넌트를 반환합니다.</summary>
        /// <param name="id">캐릭터 ID</param>
        /// <returns>생성된 Pawn 컴포넌트</returns>
        private Pawn ClonePrefab(CharacterID id)
        {
            var pawnObject = Instantiate(GetPrefabById(id));
            var pawn = pawnObject.GetComponent<Pawn>();
            return pawn;
        }

        /// <summary>
        /// ID에 해당하는 캐릭터 프리팹을 반환합니다.</summary>
        /// <param name="id">캐릭터 ID</param>
        /// <returns>해당하는 GameObject 프리팹</returns>
        private GameObject GetPrefabById(CharacterID id)
        {
            // TODO: characterID와 characterPrefab 매칭 필요
            return characterPrefabs[id];
            
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