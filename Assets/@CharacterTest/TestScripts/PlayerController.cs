using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterSystem
{
    public class PlayerController : Controller
    {
        public InputActionReference moveAction; // 에디터에서 할당
        public InputActionReference attackAction; // 에디터에서 할당

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action.Enable();
            }
            if (attackAction != null)
            {
                attackAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.Disable();
            }
        }

        public override void ProcessInput() // To-Do: 이름 변경
        {
            if (owner == null)
            {
                return;
            }

            Vector2 moveDir = Vector2.zero;
            if (moveAction != null)
            {
                moveDir = moveAction.action.ReadValue<Vector2>();
                owner.Move(moveDir);
            }

            if (attackAction != null)
            {
                if (attackAction.action.ReadValue<float>() > 0)
                {
                    owner.PerformAutoAttack();
                }
            }
        }
    }
} 