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

        public virtual void AttackOnAnimationEnd()
        {
        }
    }
}