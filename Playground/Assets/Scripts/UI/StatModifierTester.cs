using UnityEngine;
using UnityEngine.UI;
using Stats;

public class StatModifierTester : MonoBehaviour
{
    public Player player; // Player 오브젝트 Drag & Drop
    public Button flatBuffButton; // Button Drag & Drop

    private void Awake()
    {
        if (player == null)
            player = FindObjectOfType<Player>();

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