using UnityEngine;
using CharacterSystem;
using System.Collections.Generic;
using CombatSystem;
using Utils;

namespace BattleSystem
{
    public class BattleStageManager : MonoBehaviour
    {
        // ===== 공용(생성자, 싱글턴, Unity 콜백 등) =====
        public static BattleStageManager Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // 이 GameObject가 씬이 변경되어도 파괴되지 않게 하려면 DontDestroyOnLoad(gameObject); 추가
                // 하지만 전투 씬에 종속된다면 필요 없음. 다이어그램 상으로는 씬마다 생성될 것으로 보임.
            }
            else
            {
                Destroy(gameObject);
            }

            Debug.Log("BattleStageManager Awake: Initializing battle stage.");
            InitializeCharacters();
        }
        private void Update()
        {
            elapsedTime += Time.deltaTime;
            OnTick();
        }
        private void OnDestroy()
        {
            foreach (var character in characters)
            {
                if (character != null)
                {
                    CharacterPool.Instance.ReturnToPool(character);
                }
            }
            characters.Clear();
            foreach (var enemy in enemies)
            {
                if (enemy != null) enemy.gameObject.SetActive(false);
            }
            enemies.Clear();
            Debug.Log("BattleStageManager OnDestroy: Cleaning up battle objects.");
        }

        // ===== [기능 1] 전투 스테이지 데이터 및 캐릭터 관리 =====
        [Header("Battle Stage Settings")]
        public Difficulty difficulty;
        public List<Pawn> characters = new List<Pawn>();
        public List<Pawn> enemies = new List<Pawn>();
        public bool isFirstBattle = true;
        [Header("Managers & Components")]
        public SpawnManager spawnManager;
        public BattleStageView BattleStageView;
        [Header("Prefabs")]
        public GameObject characterPrefab;
        private float elapsedTime = 0f;
        private void InitializeCharacters()
        {
            if (isFirstBattle)
            {
                if (characterPrefab != null)
                {
                    GameObject playerCharacterGO = Instantiate(characterPrefab);
                    Pawn playerPawn = playerCharacterGO.GetComponent<Pawn>();
                    if (playerPawn != null)
                    {
                        characters.Add(playerPawn);
                        Debug.Log($"Selected character '{playerCharacterGO.name}' created and added to characters list.");
                    }
                    else
                    {
                        Debug.LogError($"Character prefab '{characterPrefab.name}' does not have a Pawn component.");
                    }
                }
                else
                {
                    Debug.LogError("Character Prefab is not assigned in BattleStageManager.");
                }
            }
            else
            {
                var pooledCharacters = CharacterPool.Instance.GetActiveCharacters();
                if (pooledCharacters != null && pooledCharacters.Count > 0)
                {
                    characters.AddRange(pooledCharacters);
                    Debug.Log($"Retrieved {pooledCharacters.Count} characters from pool.");
                }
                else
                {
                    Debug.LogError("No characters available in the pool for subsequent Battle.");
                }
            }
        }

        // ===== [기능 2] 시간 관리 및 틱 =====
        private void OnTick()
        {
            // 여기에 게임 로직 (예: 적 AI 갱신, 스킬 쿨타임 감소 등)을 추가
        }
        public float GetTime()
        {
            return elapsedTime;
        }

        // ===== [기능 3] 전투 결과 처리 =====
        public void Defeat()
        {
            Debug.Log("BattleStageManager: Battle Defeat!");
            // 게임 오버 UI 표시, 씬 전환, 데이터 저장 등
        }
        public void Victory()
        {
            Debug.Log("BattleStageManager: Battle Victory!");
            // 승리 UI 표시, 보상 지급, 씬 전환, 데이터 저장 등
        }
    }
} 