using UnityEngine;
using UnityEngine.UI;
using CharacterSystem;

namespace Utils
{
    /// <summary>
    /// UI 버튼을 눌렀을 때 Pawn의 OnBattleStart를 호출하는 컴포넌트
    /// </summary>
    public class BattleStartButton : MonoBehaviour
    {
        [Header("Battle Start Settings")]
        [SerializeField] private Pawn targetPawn; // OnBattleStart를 호출할 Pawn
        
        private Button button;
        
        private void Awake()
        {
            // Button 컴포넌트 가져오기
            button = GetComponent<Button>();
            
            // 버튼 클릭 이벤트에 함수 연결
            if (button != null)
            {
                button.onClick.AddListener(OnBattleStartButtonClicked);
            }
        }
        
        private void OnDestroy()
        {
            // 이벤트 연결 해제
            if (button != null)
            {
                button.onClick.RemoveListener(OnBattleStartButtonClicked);
            }
        }
        
        /// <summary>
        /// 버튼이 클릭되었을 때 호출되는 함수
        /// </summary>
        private void OnBattleStartButtonClicked()
        {
            if (targetPawn != null)
            {
                Debug.Log($"<color=green>[UI] Battle Start Button clicked! Calling OnBattleStart on {targetPawn.gameObject.name}</color>");
                targetPawn.OnBattleStart();
            }
            else
            {
                Debug.LogWarning("<color=red>[UI] Battle Start Button: targetPawn is null!</color>");
            }
        }
        
        /// <summary>
        /// 런타임에서 Pawn을 설정하는 함수
        /// </summary>
        /// <param name="pawn">설정할 Pawn</param>
        public void SetTargetPawn(Pawn pawn)
        {
            targetPawn = pawn;
        }
        
        /// <summary>
        /// Hierarchy에서 "Pawn" 태그를 가진 오브젝트를 자동으로 찾아서 설정
        /// </summary>
        [ContextMenu("Find Pawn in Hierarchy")]
        public void FindPawnInHierarchy()
        {
            Pawn foundPawn = FindFirstObjectByType<Pawn>();
            if (foundPawn != null)
            {
                targetPawn = foundPawn;
                Debug.Log($"<color=green>[UI] Found Pawn: {foundPawn.gameObject.name}</color>");
            }
            else
            {
                Debug.LogWarning("<color=red>[UI] No Pawn found in scene!</color>");
            }
        }
    }
} 