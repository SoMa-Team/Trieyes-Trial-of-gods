using UnityEngine;
using UnityEngine.InputSystem;

namespace CharacterSystem
{
    /// <summary>
    /// Unity Input System을 사용한 플레이어 입력 처리 컨트롤러
    /// </summary>
    public class PlayerController : Controller
    {
        // ===== [필드] =====
        /// <summary>
        /// 이동 입력 액션 (에디터에서 할당)
        /// </summary>
        public InputActionReference moveAction;
        
        /// <summary>
        /// 공격 입력 액션 (에디터에서 할당)
        /// </summary>
        public InputActionReference attackAction;
        
        /// <summary>
        /// 현재 이동 방향
        /// </summary>
        public Vector2 moveDir = Vector2.zero;

        // ===== [Unity 생명주기] =====
        /// <summary>
        /// 컴포넌트가 활성화될 때 호출됩니다.
        /// </summary>
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

        private void Update()
        {
            moveDir = moveAction.action.ReadValue<Vector2>();
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 입력을 처리하고 Pawn에 명령을 전달합니다.
        /// </summary>
        public override void ProcessInputActions()
        {
            if (owner == null)
            {
                return;
            }
            
            // 이동 처리
            if (moveAction != null)
            {
                owner.Move(moveDir);
            }

            // 공격 처리
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