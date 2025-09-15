using CharacterSystem;

namespace AnimationSystem
{
    public class E005_AnimationController : AnimationController
    {
        private E005_BlueGolem _owner;
        public override void Awake()
        {
            _owner = gameObject.GetComponentInParent<E005_BlueGolem>();
        }

        public override void AttackOnAnimationEnd()
        {
            _owner.Animator.SetBool("Attack", false);
        }
    }
}