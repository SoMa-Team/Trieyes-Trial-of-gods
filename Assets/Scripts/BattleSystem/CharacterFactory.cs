using System;
using UnityEngine;
using CharacterSystem;

namespace BattleSystem
{
    using CharacterID = Int32;
    
    public class CharacterFactory : MonoBehaviour
    {
        // === 싱글톤 ===
        private static CharacterFactory _instance;
        public static CharacterFactory Instance {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("CharacterFactory").AddComponent<CharacterFactory>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        // === Pawn 생성 ===
        public GameObject[] characterPrefabs;
        
        /// <summary>
        /// CharacterID에 맞는 캐릭터 gameObject를 생성합니다.
        /// gameObject는 반드시 Pawn을 상속한 Unity Component가 부착되어 있습니다.
        /// </summary>
        /// <param name="id">Character ID가 주어집니다</param>
        /// <returns>gameObject에 부착된 Pawn 객체를 반환합니다.</returns>
        public Pawn Create(CharacterID id)
        {
            var pawn = ClonePrefab(id);
            return pawn;
        }
        
        private Pawn ClonePrefab(CharacterID id)
        {
            var pawnObject = Instantiate(GetPrefabById(id));
            var pawn = pawnObject.AddComponent<Pawn>();
            return pawn;
        }

        private GameObject GetPrefabById(CharacterID id)
        {
            return id switch
            {
                0 => characterPrefabs[0],
                1 => characterPrefabs[1],
                // ...
                _ => null
            };
        }
    }
}