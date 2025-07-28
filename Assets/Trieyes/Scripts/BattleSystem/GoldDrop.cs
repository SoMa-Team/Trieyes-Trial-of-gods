using System;
using BattleSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using UnityEngine.WSA;

public class GoldDrop : MonoBehaviour
{
    private bool isActive = false;
    [SerializeField] private int goldAmount;

    private void Update()
    {
        if (!isActive)
            return;

        var character = BattleStage.now.mainCharacter;
        var position = transform.position;
        var characterPosition = character.transform.position;
        var distance = Vector3.Distance(characterPosition, position);

        if (distance < GetGoldCollisionDistance(BattleStage.now.mainCharacter))
        {
            character.ChangeGold(goldAmount);
            GoldDropFactory.Instance.Deactivate(this);
        }
    }

    private float GetGoldCollisionDistance(Pawn pawn)
    {
        var magnet = pawn.statSheet[StatType.ItemMagnet].Value;
        var resultDistance = 0.2f + MathF.Log(magnet + 1);
        return resultDistance;
    }

    public void Activate(int goldAmount)
    {
        isActive = true;
        this.goldAmount = goldAmount;
    }

    public void Deactivate()
    {
        isActive = false;
        goldAmount = 0;
    }
}
