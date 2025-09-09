using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NodeStage
{
    public class NextStageSlot : MonoBehaviour
    {
        [Header("UI Refs")]
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI label;

        private StageType stageType;
        private Action<StageType> onPick;

        /// <summary>슬롯 내용을 채우고 클릭 핸들러를 바인딩합니다.</summary>
        public void SetStage(StageInfoSO info, Action<StageType> onPick)
        {
            stageType = info.type;
            this.onPick = onPick;

            if (icon)  icon.sprite = info.icon;
            if (label) label.text  = info.name;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);

            gameObject.SetActive(true);
        }

        private void HandleClick()
        {
            onPick?.Invoke(stageType);
        }

        /// <summary>리스너 해제 등 정리 후 비활성화.</summary>
        public void Deactivate()
        {
            button.onClick.RemoveAllListeners();
            onPick = null;
            gameObject.SetActive(false);
        }
    }
}