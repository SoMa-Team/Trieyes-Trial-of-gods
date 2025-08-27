using UnityEngine;
using BattleSystem;
using CharacterSystem;
using AttackSystem;

public class AnimationController : MonoBehaviour
{
    private Pawn owner;
    public void Awake()
    {
        owner = gameObject.GetComponentInParent<Pawn>();
    }
    public void DestroyOnDeath()
    {
        if (!owner.isEnemy)
        {
            CharacterFactory.Instance.Deactivate(owner);   
        }
        else
        {
            EnemyFactory.Instance.Deactivate(owner as Enemy);
        }
    }

    public void BasicAttackOnAnimationStart()
    {
        if (owner is Character character)
        {
            character.CreateAttack(PawnAttackType.BasicAttack);
        }
    }

    public void Skill1OnAnimationStart()
    {
        if (owner is Character character)
        {
            character.CreateAttack(PawnAttackType.Skill1);
        }
    }

    public void Skill2OnAnimationStart()
    {
        if (owner is Character character)
        {
            character.CreateAttack(PawnAttackType.Skill2);
        }
    }

    public void AttackOnAnimationEnd()
    {
    }
}