using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using GameFramework;
using CharacterSystem;

namespace NodeStage
{
    public class NextStageSelectPopup : MonoBehaviour
    {
        public List<StageInfoSO> allStages;
        
        [SerializeField] private NextStageSlot slotPrefab;
        [SerializeField] private Transform     slotContainer;
        [SerializeField] private RectTransform rectTransform;
        public static NextStageSelectPopup Instance { get; private set; }
        
        private readonly List<NextStageSlot> spawnedSlots = new();
        private System.Random rng = new System.Random();
        
        private static readonly HashSet<StageType> StartTypes = new()
        {
            StageType.StartCard,
            StageType.StartRelic,
        };
        
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
        }

        private List<T> SampleWithoutReplacement<T>(IList<T> src, int k, System.Random r)
        {
            if (src == null || src.Count == 0 || k <= 0) return new List<T>();
            k = Mathf.Clamp(k, 0, src.Count);
            var arr = new List<T>(src);
            for (int i = 0; i < k; i++)
            {
                int j = r.Next(i, arr.Count);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            return arr.GetRange(0, k);
        }
        
        private StageInfoSO GetFirstInfoOfType(StageType t)
        {
            return allStages
                .FirstOrDefault(s => s != null && s.type == t);
        }
        
        private void SpawnSlots(IEnumerable<StageInfoSO> infos, Character mainCharacter)
        {
            ClearSlots();
            foreach (var info in infos)
            {
                if (info == null) continue;
                var slot = Instantiate(slotPrefab, slotContainer);
                slot.Activate(info, chosenType =>
                {
                    InGameManager.Instance.StartNextStage(chosenType, mainCharacter);
                    Deactivate();
                });
                spawnedSlots.Add(slot);
            }
            gameObject.SetActive(true);
        }
        
        public void Deactivate()
        {
            ClearSlots();
            gameObject.SetActive(false);
        }

        private void ClearSlots()
        {
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                if (spawnedSlots[i] != null)
                {
                    spawnedSlots[i].Deactivate();
                    Destroy(spawnedSlots[i].gameObject);
                }
            }
            spawnedSlots.Clear();
        }
        public void SetNextStage(StageType current, Character mainCharacter)
        {
            this.gameObject.SetActive(true);
            ClearSlots();
            
            var pool = allStages
                .Where(s => s != null)
                .GroupBy(s => s.type)
                .Select(g => g.First())
                .Where(s => s.type != current && !StartTypes.Contains(s.type))
                .ToList();
            
            var options = SampleWithoutReplacement(pool, 3, rng);
            
            SpawnSlots(options, mainCharacter);
        }
        
        public void StartGame(Character mainCharacter)
        {
            ClearSlots();
            ShowStartChoices(mainCharacter);
        }

        private void ShowStartChoices(Character mainCharacter)
        {
            var startInfos = new List<StageInfoSO>
                {
                    GetFirstInfoOfType(StageType.StartCard),
                    GetFirstInfoOfType(StageType.StartRelic),
                }
                .Where(x => x != null)
                .ToList();
            
            SpawnSlots(startInfos, mainCharacter);
        }
    }
}