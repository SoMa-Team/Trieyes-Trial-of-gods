using CharacterSystem;
using GameFramework;
using UnityEngine;

namespace NodeStage
{
    public class CampFireStage : MonoBehaviour, NodeStage
    {
        [SerializeField] private RectTransform rectTransform;
        private Character mainCharacter;
        
        public static CampFireStage Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
        }
        
        public void Activate(Character mainCharacter)
        {
            this.mainCharacter = mainCharacter;
            this.gameObject.SetActive(true);
        }

        private void DeActivate()
        {
            this.gameObject.SetActive(false);
        }

        public void NextStage()
        {
            DeActivate();
            NextStageSelectPopup.Instance.SetNextStage(StageType.CampFire, mainCharacter);
        }
    }
}