using UnityEngine;
using System.Collections.Generic;
using CharacterSystem;

namespace CombatSystem
{
    public class CharacterPool : MonoBehaviour
    {
        public static CharacterPool Instance { get; private set; }

        private List<Pawn> activeCharacters = new List<Pawn>();
        private List<Pawn> inactiveCharacters = new List<Pawn>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 현재 활성화된 캐릭터들을 반환합니다.
        /// </summary>
        public List<Pawn> GetActiveCharacters()
        {
            return new List<Pawn>(activeCharacters);
        }

        /// <summary>
        /// 캐릭터를 풀에 추가합니다.
        /// </summary>
        public void AddToPool(Pawn character)
        {
            if (character != null && !activeCharacters.Contains(character))
            {
                activeCharacters.Add(character);
                DontDestroyOnLoad(character.gameObject);
            }
        }

        /// <summary>
        /// 캐릭터를 풀에 반환합니다.
        /// </summary>
        public void ReturnToPool(Pawn character)
        {
            if (character != null)
            {
                if (activeCharacters.Contains(character))
                {
                    activeCharacters.Remove(character);
                }
                inactiveCharacters.Add(character);
                character.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 풀에서 캐릭터를 가져옵니다.
        /// </summary>
        public Pawn GetFromPool()
        {
            if (inactiveCharacters.Count > 0)
            {
                var character = inactiveCharacters[0];
                inactiveCharacters.RemoveAt(0);
                activeCharacters.Add(character);
                character.gameObject.SetActive(true);
                return character;
            }
            return null;
        }

        /// <summary>
        /// 모든 캐릭터를 비활성화합니다.
        /// </summary>
        public void DeactivateAllCharacters()
        {
            foreach (var character in activeCharacters)
            {
                if (character != null)
                {
                    character.gameObject.SetActive(false);
                    inactiveCharacters.Add(character);
                }
            }
            activeCharacters.Clear();
        }
    }
} 