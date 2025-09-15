using CharacterSystem;

namespace AnimationSystem
{
    public class E005_AnimationController : AnimationController
    {
        private E005_BlueGolem owner;
        public override void Awake()
        {
            owner = gameObject.GetComponentInParent<E005_BlueGolem>();
        }

        public override void AttackOnAnimationEnd()
        {
            owner.Animator.SetBool("Attack", false);
        }
    }
}