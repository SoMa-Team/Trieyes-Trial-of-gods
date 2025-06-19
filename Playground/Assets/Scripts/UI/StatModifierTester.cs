using UnityEngine;
using UnityEngine.UI;
using Stats;

public class StatModifierTester : MonoBehaviour
{
    public Player player; 
    public Button flatBuffButton; 

    private void Awake()
    {
        if (player == null)
            player = Object.FindFirstObjectByType<Player>(); // 최신 방식

        if (flatBuffButton != null)
            flatBuffButton.onClick.AddListener(ApplyFlatBuff);
    }

    public void ApplyFlatBuff()
    {
        var buff = new StatModifier(
            15, BuffOperationType.Additive, true, 5f
        );
        player.statSheet[StatType.AttackPower].AddBuff(buff);
        Debug.Log("Flat Buff (+15 for 5s) applied!");
    }
}