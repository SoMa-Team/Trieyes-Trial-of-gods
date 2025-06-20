using UnityEngine;
using UnityEngine.UI;
using CharacterSystem;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// 여러 Pawn에 OnBattleStart를 호출할 수 있는 버튼 컴포넌트
    /// </summary>
    public class MultiPawnBattleButton : MonoBehaviour
    {
        [Header("Battle Start Settings")]
        [SerializeField] private List<Pawn> targetPawns = new List<Pawn>(); // 여러 Pawn을 저장
        [SerializeField] private bool findAllPawnsAutomatically = true; // 자동으로 모든 Pawn 찾기
        
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
            
            if (button != null)
            {
                button.onClick.AddListener(OnBattleStartButtonClicked);
            }
            
            // 자동으로 모든 Pawn 찾기
            if (findAllPawnsAutomatically)
            {
                FindAllPawnsInScene();
            }
        }
        
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnBattleStartButtonClicked);
            }
        }
        
        private void OnBattleStartButtonClicked()
        {
            if (targetPawns.Count == 0)
            {
                Debug.LogWarning("<color=red>[UI] No Pawns to call OnBattleStart!</color>");
                return;
            }
            
            Debug.Log($"<color=green>[UI] Battle Start Button clicked! Calling OnBattleStart on {targetPawns.Count} Pawns</color>");
            
            foreach (var pawn in targetPawns)
            {
                if (pawn != null)
                {
                    pawn.OnBattleStart();
                }
            }
        }
        
        /// <summary>
        /// 씬의 모든 Pawn을 찾아서 리스트에 추가
        /// </summary>
        [ContextMenu("Find All Pawns in Scene")]
        public void FindAllPawnsInScene()
        {
            Pawn[] allPawns = FindObjectsByType<Pawn>(FindObjectsSortMode.None);
            targetPawns.Clear();
            targetPawns.AddRange(allPawns);
            
            Debug.Log($"<color=green>[UI] Found {allPawns.Length} Pawns in scene</color>");
        }
        
        /// <summary>
        /// 특정 Pawn 추가
        /// </summary>
        public void AddPawn(Pawn pawn)
        {
            if (pawn != null && !targetPawns.Contains(pawn))
            {
                targetPawns.Add(pawn);
            }
        }
        
        /// <summary>
        /// 특정 Pawn 제거
        /// </summary>
        public void RemovePawn(Pawn pawn)
        {
            targetPawns.Remove(pawn);
        }
        
        /// <summary>
        /// 모든 Pawn 제거
        /// </summary>
        public void ClearPawns()
        {
            targetPawns.Clear();
        }
    }
} 