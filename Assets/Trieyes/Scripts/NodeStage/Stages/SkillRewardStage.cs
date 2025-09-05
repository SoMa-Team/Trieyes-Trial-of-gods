using CharacterSystem;
using GameFramework;
using UnityEngine;

namespace NodeStage
{
    public class SkillRewardStage : MonoBehaviour, NodeStage
    {
        private Character mainCharacter;
        
        public static SkillRewardStage Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
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
            NextStageSelectPopup.Instance.SetNextStage(StageType.SkillReward, mainCharacter);
        }
    }
}