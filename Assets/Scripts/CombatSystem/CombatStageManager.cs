using UnityEngine;
using CharacterSystem;
using System.Collections.Generic;
using GameFramework;

namespace CombatSystem
{
    public class CombatStageManager : MonoBehaviour
    {
        public static CombatStageManager Instance { get; private set; }

        [Header("Combat Stage Settings")]
        public Difficulty difficulty; // 전투 난이도
        public List<Pawn> characters = new List<Pawn>(); // 플레이어 조종 캐릭터 후보군
        public List<Pawn> enemies = new List<Pawn>(); // 현재 전투에 참여하는 모든 적들

        [Header("Managers & Components")]
        public SpawnManager spawnManager; // 적 스폰 관리자
        public CombatStageView combatStageView; // 전투 씬 UI 및 시각적 요소 관리자

        [Header("Prefabs")]
        public GameObject character001Prefab; // Character001 프리팹 할당용

        // 전투 시간 관리 (예시: 유니티의 시간 함수 등과 연동될 기준점 데이터)
        private float elapsedTime = 0f;

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

            // 다이어그램 3: CombatStage가 전투에 필요한 GameObject들 생성
            // 필드 초기화 (예: SpawnerManager 인스턴스 할당 등)
            // Pawn 계열 (적, 캐릭터) 생성 (Instantiate 또는 풀링에서 불러오기)
            // UI 요소 설정

            Debug.Log("CombatStageManager Awake: Initializing battle stage.");

            // Character001 생성
            if (character001Prefab != null)
            {
                GameObject playerCharacterGO = Instantiate(character001Prefab);
                Pawn playerPawn = playerCharacterGO.GetComponent<Pawn>();
                if (playerPawn != null)
                {
                    characters.Add(playerPawn);
                    Debug.Log($"Character001 '{playerCharacterGO.name}' created and added to characters list.");
                }
                else
                {
                    Debug.LogError($"Character001 prefab '{character001Prefab.name}' does not have a Pawn component.");
                }
            }
            else
            {
                Debug.LogError("Character001 Prefab is not assigned in CombatStageManager.");
            }

            // 예시: 적 생성 (실제 구현은 SpawnManager와 EnemyFactory 사용)
            // enemies.Add(Instantiate(someEnemyPrefab).GetComponent<Pawn>());
        }

        private void Update()
        {
            // 다이어그램 6: CombatStage.Update()에서 시간 관리해서 OnTick 호출
            elapsedTime += Time.deltaTime;
            OnTick(); // 시간 경과에 따른 로직 처리
        }

        /// <summary>
        /// 매 프레임 또는 특정 시간 간격으로 호출되어야 하는 로직 (예: 적 AI 갱신, 타이머 등)
        /// </summary>
        private void OnTick()
        {
            // 여기에 게임 로직 (예: 적 AI 갱신, 스킬 쿨타임 감소 등)을 추가
        }

        /// <summary>
        /// 현재 전투 레벨에서의 시간 기준점 데이터를 반환합니다.
        /// </summary>
        public float GetTime()
        {
            return elapsedTime;
        }

        /// <summary>
        /// 전투 패배 시 호출되는 로직입니다. 캐릭터나 각종 오브젝트에서 이벤트 발동 시 이곳에서 처리 됩니다.
        /// </summary>
        public void Defeat()
        {
            Debug.Log("CombatStageManager: Battle Defeat!");
            // 게임 오버 UI 표시, 씬 전환, 데이터 저장 등
            // 다이어그램: OnDestroy()에서 소유 객체 정리, StoreStageManager로 캐릭터 넘김
            // SceneChangeManager.Instance.LoadScene("StoreScene", characters);
        }

        /// <summary>
        /// 전투 승리 시 호출되는 로직입니다. 캐릭터나 각종 오브젝트에서 이벤트 발동 시 이곳에서 처리 됩니다.
        /// </summary>
        public void Victory()
        {
            Debug.Log("CombatStageManager: Battle Victory!");
            // 승리 UI 표시, 보상 지급, 씬 전환, 데이터 저장 등
            // SceneChangeManager.Instance.LoadScene("StoreScene", characters);
        }

        private void OnDestroy()
        {
            // 다이어그램: 전투 클리어 / 전투 패배 시 OnDestroy()에서 소유 객체 정리
            // Pawn들은 InActive, Attack들도 InActive (풀링 개념)
            foreach (var character in characters)
            {
                if (character != null) character.gameObject.SetActive(false);
            }
            foreach (var enemy in enemies)
            {
                if (enemy != null) enemy.gameObject.SetActive(false);
            }
            // Attack 객체들도 관리하고 있다면 비활성화 로직 추가

            Debug.Log("CombatStageManager OnDestroy: Cleaning up battle objects.");

            // StoreStageManager로 캐릭터 넘김은 실제 씬 전환 로직에서 처리하는 것이 더 적합할 수 있음
            // SceneChangeManager.Instance.LoadScene("StoreScene", characters); 
        }
    }
} 