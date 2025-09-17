using UnityEngine;
using CharacterSystem;

namespace AnimationSystem
{
    public class AnimationController : MonoBehaviour
    {
        private Pawn owner;
        public virtual void Awake()
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

        public virtual void BasicAttackOnAnimationStart()
        {
            if (owner is Character character)
            {
                character.CreateAttack(PawnAttackType.BasicAttack);
            }
            if (owner is Enemy enemy)
            {
                enemy.CreateAttack(PawnAttackType.BasicAttack);
            }
        }

        public virtual void Skill1OnAnimationStart()
        {
            if (owner is Character character)
            {
                character.CreateAttack(PawnAttackType.Skill1);
            }
            if (owner is Enemy enemy)
            {
                enemy.CreateAttack(PawnAttackType.Skill1);
            }
        }

        public virtual void Skill2OnAnimationStart()
        {
            if (owner is Character character)
            {
                character.CreateAttack(PawnAttackType.Skill2);
            }
            if (owner is Enemy enemy)
            {
                enemy.CreateAttack(PawnAttackType.Skill2);
            }
        }

        public virtual void AttackOnAnimationEnd()
        {
        }
    }
}