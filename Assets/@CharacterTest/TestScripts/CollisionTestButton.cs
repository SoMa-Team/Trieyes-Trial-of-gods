using UnityEngine;
using UnityEngine.UI;
using AttackSystem;
using CharacterSystem;
using System.Reflection;
using Stats;

// TODO: 테스트 완료 후 제거
namespace Utils
{
    /// <summary>
    /// Attack과 Pawn 사이의 충돌 이벤트를 검증하기 위한 테스트 버튼입니다.
    /// </summary>
    public class CollisionTestButton : MonoBehaviour
    {
        [Header("테스트 대상")]
        [SerializeField] private GameObject attackPrefab; // Attack 프리팹
        [SerializeField] private GameObject pawnPrefab; // Pawn 프리팹
        
        [Header("UI 설정")]
        [SerializeField] private Button testButton; // 테스트 버튼
        [SerializeField] private Button openLogButton; // 로그 파일 열기 버튼
        [SerializeField] private Button clearLogButton; // 로그 파일 삭제 버튼
        
        private void Start()
        {
            // 버튼 이벤트 연결
            if (testButton != null)
            {
                testButton.onClick.AddListener(OnTestButtonClicked);
            }
            else
            {
                // 버튼이 할당되지 않은 경우 자동으로 찾기
                testButton = GetComponent<Button>();
                if (testButton != null)
                {
                    testButton.onClick.AddListener(OnTestButtonClicked);
                }
            }
            
            // 로그 파일 열기 버튼
            if (openLogButton != null)
            {
                openLogButton.onClick.AddListener(OnOpenLogButtonClicked);
            }
            
            // 로그 파일 삭제 버튼
            if (clearLogButton != null)
            {
                clearLogButton.onClick.AddListener(OnClearLogButtonClicked);
            }
            
            Debug.Log("<color=green>[CollisionTestButton] 충돌 테스트 버튼이 초기화되었습니다.</color>");
            
            // LogToFile 시스템이 없으면 생성
            if (LogToFile.Instance == null)
            {
                GameObject logToFileObj = new GameObject("LogToFile");
                logToFileObj.AddComponent<LogToFile>();
                Debug.Log("<color=green>[CollisionTestButton] LogToFile 시스템이 자동으로 생성되었습니다.</color>");
            }
        }
        
        /// <summary>
        /// 테스트 버튼 클릭 시 호출되는 메서드입니다.
        /// </summary>
        public void OnTestButtonClicked()
        {
            Debug.Log("<color=yellow>[CollisionTestButton] 충돌 테스트를 시작합니다...</color>");
            
            // 프리팹에서 바로 OnCollisionEnter2D 실행
            TriggerAttackCollisions();
        }
        
        /// <summary>
        /// 로그 파일 열기 버튼 클릭 시 호출되는 메서드입니다.
        /// </summary>
        public void OnOpenLogButtonClicked()
        {
            if (LogToFile.Instance != null)
            {
                LogToFile.Instance.OpenLogFile();
                Debug.Log("<color=cyan>[CollisionTestButton] 로그 파일을 열었습니다.</color>");
            }
            else
            {
                Debug.LogError("<color=red>[CollisionTestButton] LogToFile 시스템이 없습니다!</color>");
            }
        }
        
        /// <summary>
        /// 로그 파일 삭제 버튼 클릭 시 호출되는 메서드입니다.
        /// </summary>
        public void OnClearLogButtonClicked()
        {
            if (LogToFile.Instance != null)
            {
                LogToFile.Instance.ClearLogFile();
                Debug.Log("<color=cyan>[CollisionTestButton] 로그 파일을 삭제했습니다.</color>");
            }
            else
            {
                Debug.LogError("<color=red>[CollisionTestButton] LogToFile 시스템이 없습니다!</color>");
            }
        }
        
        /// <summary>
        /// 각 투사체(Attack)의 OnCollisionEnter2D를 발동시킵니다.
        /// </summary>
        private void TriggerAttackCollisions()
        {
            // 프리팹이 없으면 자동으로 생성
            if (attackPrefab == null)
            {
                Debug.LogWarning("<color=yellow>[CollisionTestButton] Attack 프리팹이 할당되지 않아 자동으로 생성합니다.</color>");
                attackPrefab = CreateDefaultAttackPrefab();
            }
            
            if (pawnPrefab == null)
            {
                Debug.LogWarning("<color=yellow>[CollisionTestButton] Pawn 프리팹이 할당되지 않아 자동으로 생성합니다.</color>");
                pawnPrefab = CreateDefaultPawnPrefab();
            }
            
            Debug.Log($"<color=cyan>[CollisionTestButton] 관리자 Attack 프리팹: {attackPrefab.name}</color>");
            Debug.Log($"<color=cyan>[CollisionTestButton] Pawn 프리팹: {pawnPrefab.name}</color>");
            
            // 관리자 Attack 프리팹에서 모든 하위 투사체(Attack)들 찾기
            Attack[] allAttacks = attackPrefab.GetComponentsInChildren<Attack>();
            Debug.Log($"<color=cyan>[CollisionTestButton] 발견된 하위 투사체(Attack) 개수: {allAttacks.Length}</color>");
            
            if (allAttacks.Length == 0)
            {
                Debug.LogError("<color=red>[CollisionTestButton] 관리자 Attack 프리팹에서 하위 투사체(Attack)를 찾을 수 없습니다!</color>");
                return;
            }
            
            // Pawn 컴포넌트 가져오기
            Pawn pawnComponent = pawnPrefab.GetComponent<Pawn>();
            if (pawnComponent == null)
            {
                Debug.LogError("<color=red>[CollisionTestButton] Pawn 프리팹에서 Pawn 컴포넌트를 찾을 수 없습니다!</color>");
                return;
            }
            
            Debug.Log($"<color=green>[CollisionTestButton] Pawn 컴포넌트를 성공적으로 찾았습니다!</color>");
            
            // 각 하위 투사체(Attack)에 대해 OnCollisionEnter2D 호출
            for (int i = 0; i < allAttacks.Length; i++)
            {
                Attack attack = allAttacks[i];
                Debug.Log($"<color=blue>[CollisionTestButton] 투사체(Attack) {i + 1}: {attack.name}</color>");
                
                // 각 투사체(Attack)의 OnCollisionEnter2D 직접 호출
                TriggerAttackOnCollision(attack, pawnPrefab);
            }
        }
        
        /// <summary>
        /// 기본 Attack 프리팹을 생성합니다.
        /// </summary>
        /// <returns>생성된 Attack 프리팹</returns>
        private GameObject CreateDefaultAttackPrefab()
        {
            // 관리자 Attack 오브젝트 생성
            GameObject attackManager = new GameObject("DefaultAttackManager");
            
            // 하위에 실제 투사체 Attack 생성
            GameObject projectile = new GameObject("DefaultProjectile");
            projectile.transform.SetParent(attackManager.transform);
            
            // Attack 컴포넌트 추가
            Attack attackComponent = projectile.AddComponent<Attack>();
            
            // 기본 물리 컴포넌트 추가
            Rigidbody2D rb = projectile.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            
            CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
            collider.isTrigger = false;
            
            // 기본 AttackData 설정
            AttackData attackData = new AttackData();
            attackData.attackId = 1;
            attackData.attackName = "DefaultAttack";
            attackData.attackType = AttackType.Basic;
            attackData.cooldown = 10f;
            attackData.bIsActivated = true;
            attackData.statSheet = new StatSheet();
            
            // Attack 컴포넌트에 AttackData 설정
            attackComponent.attackData = attackData;
            
            Debug.Log("<color=green>[CollisionTestButton] 기본 Attack 프리팹이 생성되었습니다.</color>");
            return attackManager;
        }
        
        /// <summary>
        /// 기본 Pawn 프리팹을 생성합니다.
        /// </summary>
        /// <returns>생성된 Pawn 프리팹</returns>
        private GameObject CreateDefaultPawnPrefab()
        {
            // Pawn 오브젝트 생성
            GameObject pawnObject = new GameObject("DefaultPawn");
            
            // Pawn 컴포넌트 추가 (Character001 사용)
            Character001 pawnComponent = pawnObject.AddComponent<Character001>();
            
            // 기본 물리 컴포넌트 추가
            Rigidbody2D rb = pawnObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            
            BoxCollider2D collider = pawnObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = false;
            
            // 기본 스탯 설정 (Character001.Awake에서 자동으로 설정됨)
            
            Debug.Log("<color=green>[CollisionTestButton] 기본 Pawn 프리팹이 생성되었습니다.</color>");
            return pawnObject;
        }
        
        /// <summary>
        /// 투사체(Attack)의 충돌 처리를 직접 호출합니다.
        /// </summary>
        /// <param name="attack">투사체(Attack) 컴포넌트</param>
        /// <param name="pawnObject">충돌할 Pawn 오브젝트</param>
        private void TriggerAttackOnCollision(Attack attack, GameObject pawnObject)
        {
            Debug.Log($"<color=orange>[CollisionTestButton] 투사체 {attack.name}과 {pawnObject.name}의 충돌을 시뮬레이션합니다...</color>");
            
            // HandleCollision 메서드를 직접 호출 (실제 충돌 데이터 시뮬레이션)
            MethodInfo handleCollisionMethod = typeof(Attack).GetMethod("HandleCollision", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            
            if (handleCollisionMethod != null)
            {
                handleCollisionMethod.Invoke(attack, new object[] { pawnObject });
                Debug.Log($"<color=green>[CollisionTestButton] 투사체 {attack.name}의 충돌 처리 완료</color>");
            }
            else
            {
                Debug.LogError($"<color=red>[CollisionTestButton] 투사체 {attack.name}의 HandleCollision 메서드를 찾을 수 없습니다!</color>");
            }
        }
        
        private void OnDestroy()
        {
            // 버튼 이벤트 연결 해제
            if (testButton != null)
            {
                testButton.onClick.RemoveListener(OnTestButtonClicked);
            }
            
            if (openLogButton != null)
            {
                openLogButton.onClick.RemoveListener(OnOpenLogButtonClicked);
            }
            
            if (clearLogButton != null)
            {
                clearLogButton.onClick.RemoveListener(OnClearLogButtonClicked);
            }
        }
    }
} 