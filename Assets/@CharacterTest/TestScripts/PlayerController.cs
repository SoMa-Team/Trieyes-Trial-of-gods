using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterSystem
{
    public class PlayerController : Controller
    {
        public InputActionReference moveAction; // 에디터에서 할당

        private void OnEnable()
        {
            if (moveAction != null)
            {
                moveAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (moveAction != null)
            {
                moveAction.action.Disable();
            }
        }

        public override void ProcessInput()
        {
            Vector2 moveDir = Vector2.zero;
            if (moveAction != null)
            {
                moveDir = moveAction.action.ReadValue<Vector2>();
            }

            if (owner != null)
            {
                owner.Move(moveDir);
            }
        }
    }
} 