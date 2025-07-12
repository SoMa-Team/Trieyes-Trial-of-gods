using System;
using AttackSystem;
using CharacterSystem;
using UnityEngine;

public class TestSkillSwapController : MonoBehaviour
{
    private Pawn pawn;
    public AttackData q;
    public AttackData w;
    public AttackData e;

    private void Start()
    {
        pawn = GetComponent<Pawn>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            pawn.basicAttack = q;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            pawn.basicAttack = w;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            pawn.basicAttack = e;
        }
    }
}
