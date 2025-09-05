using CharacterSystem;
using GameFramework;
using UnityEngine;

namespace NodeStage
{
    public class BattleRewardStage : MonoBehaviour, NodeStage
    {
        private Character mainCharacter;
        
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
            NextStageSelectPopup.Instance.SetNextStage(StageType.BattleReward, mainCharacter);
        }
    }
}