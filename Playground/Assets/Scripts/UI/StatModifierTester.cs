using System;
using UnityEngine;
using UnityEngine.UI;
using Stats;
using Object = UnityEngine.Object;

namespace UI
{
    public class StatModifierTester : MonoBehaviour
    {
        public Player player; 
        public Button flatBuffButton; 
        public Button uniqueFlatBuffButton;
        public Button percentageBuffButton;

        private StatModifier uniqueBuff = new StatModifier(30, BuffOperationType.Additive, false, 5f);
        private void Awake()
        {
            if (player == null)
                player = Object.FindFirstObjectByType<Player>(); // 최신 방식

            if (flatBuffButton != null)
                flatBuffButton.onClick.AddListener(ApplyFlatBuff);
        
            if (uniqueFlatBuffButton != null)
                uniqueFlatBuffButton.onClick.AddListener(ApplyUniqueBuff);
            if(percentageBuffButton != null)
                percentageBuffButton.onClick.AddListener(ApplyPercentageBuff);
        }

        public void ApplyFlatBuff()
        {
            var buff = new StatModifier(
                15, BuffOperationType.Additive, true, 5f
            );
            player.statSheet[StatType.AttackPower].AddBuff(buff);
            Debug.Log("Flat Buff (+15 for 5s) applied!");
        }
    
        public void ApplyUniqueBuff()
        {
            uniqueBuff.endTime = GameManager.instance.gameTime + 5f;
            player.statSheet[StatType.AttackPower].AddBuff(uniqueBuff);
            Debug.Log("Unique Buff (+30 for 5s, canStack=false) applied!");
        }

        public void ApplyPercentageBuff()
        {
            var buff = new StatModifier(
                10, BuffOperationType.Multiplicative, true, 5f);
            player.statSheet[StatType.AttackPower].AddBuff(buff);
            Debug.Log("Percentage Buff (+10 for 5s) applied!");
        }
    }
}