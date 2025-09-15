using UnityEngine;
using BattleSystem; 
using UnityEngine.UI;
using Stats;

namespace CharacterSystem
{
    public class DevUI : MonoBehaviour
    {
        public Button skipStageButton;
        public Button addTimeButton;

        public Button addAttack10000Button;
        public Button addAttackSpeed10000Button;

        public Button addSpeed10000Button;

        void Start()
        {
            skipStageButton.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.OnBattleClear(); });
            addTimeButton.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.AddTime(9999f); });
            addAttack10000Button.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.mainCharacter.statSheet[StatType.AttackPower].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.AttackPower) + 10000); });
            addAttackSpeed10000Button.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.mainCharacter.statSheet[StatType.AttackSpeed].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.AttackSpeed) + 10000); });
            addSpeed10000Button.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.mainCharacter.statSheet[StatType.MoveSpeed].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.MoveSpeed) + 10000); });
        }
    }
} 
