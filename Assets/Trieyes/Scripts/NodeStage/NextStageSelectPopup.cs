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
    [Serializable]
    public class OptionUI
    {
        public Button button;
        public Image icon;
        public TextMeshProUGUI label;
        public StageType stageType;
    }
    public class NextStageSelectPopup : MonoBehaviour
    {
        public List<StageInfoSO> allStages;
        public OptionUI[] optionSlots = new OptionUI[3];
        
        private List<StageInfoSO> currentOptions = new List<StageInfoSO>();
        private System.Random rng = new System.Random();

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

        public void SetNextStage(StageType current, Character mainCharacter)
        {
            var pool = allStages
                .Where(s => s != null)
                .GroupBy(s => s.type)
                .Select(g => g.First())
                .Where(s => s.type != current)
                .ToList();
            
            currentOptions.Clear();
            currentOptions.AddRange(SampleWithoutReplacement(pool, 3, rng));

            for (int i = 0; i < optionSlots.Length; i++)
            {
                var slot = optionSlots[i];
                var info = currentOptions[i];
                slot.stageType = info.type;
                slot.icon.sprite = info.icon;
                slot.label.text = info.name;
                slot.button.onClick.AddListener(() =>
                {
                    this.gameObject.SetActive(false);
                    InGameManager.Instance.StartNextStage(info.type, mainCharacter);
                });
            }
        }
    }
}