using System;
using UnityEngine;
using BattleSystem; 
using UnityEngine.UI;
using Stats;
using TestOnly;

namespace Utils
{
    public class DevUI : MonoBehaviour
    {
        public Button skipStageButton;
        public Button addTimeButton;

        public Button addAttack10000Button;
        public Button addAttackSpeed10000Button;

        public Button addSpeed10000Button;

        public Button gameOverButton;
        
        public Button relicChangeButton;

        void Start()
        {
            skipStageButton.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.OnBattleClear(); });
            addTimeButton.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.AddTime(9999f); });
            addAttack10000Button.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.mainCharacter.statSheet[StatType.AttackPower].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.AttackPower) + 10000); BattleStage.now.mainCharacter.statSheet[StatType.MagicPower].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.MagicPower) + 10000);});
            addAttackSpeed10000Button.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.mainCharacter.statSheet[StatType.AttackSpeed].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.AttackSpeed) + 10000); });
            addSpeed10000Button.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.mainCharacter.statSheet[StatType.MoveSpeed].SetBasicValue(BattleStage.now.mainCharacter.statSheet.GetRaw(StatType.MoveSpeed) + 10000); });
            gameOverButton.onClick.AddListener(() => { if (BattleStage.now != null) BattleStage.now.OnPlayerDeath(); });
            
            relicChangeButton.onClick.AddListener(async () =>
            {
                try
                {
                    await DevUIRelicChangePopup.Instance.Create();
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            });
        }
    }
} 
