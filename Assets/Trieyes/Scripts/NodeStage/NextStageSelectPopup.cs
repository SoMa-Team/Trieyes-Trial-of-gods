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
        public static NextStageSelectPopup Instance { get; private set; }
        
        private readonly List<NextStageSlot> spawnedSlots = new();
        private System.Random rng = new System.Random();
        
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            gameObject.SetActive(false);
        }

        private static List<T> SampleWithoutReplacement<T>(IList<T> src, int k, System.Random r)
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
                .Where(s => s.type != current)
                .ToList();
            
            var options = SampleWithoutReplacement(pool, 3, rng);
            
            foreach (var info in options)
            {
                var slot = Instantiate(slotPrefab, slotContainer);
                slot.SetStage(info, chosenType =>
                {
                    InGameManager.Instance.StartNextStage(chosenType, mainCharacter);
                    Deactivate();
                });
                spawnedSlots.Add(slot);
            }

            gameObject.SetActive(true);
        }
    }
}