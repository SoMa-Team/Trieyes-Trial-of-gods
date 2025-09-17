using System;
using UISystem;
using UnityEngine;
using UnityEngine.InputSystem;
// using UnityEngine.InputSystem; // 더 이상 필요 없음

namespace CharacterSystem
{
    /// <summary>
    /// Unity Input System을 사용한 플레이어 입력 처리 컨트롤러
    /// </summary>
    public class PlayerController : Controller
    {
        // ===== [필드] =====
        // public InputActionReference moveAction;
        // public InputActionReference attackAction;
        // public Vector2 moveDir = Vector2.zero;
        public Character character;
        public InputActionReference skillAction001;
        public InputActionReference skillAction002;

        public Joystick joystick; // 인스펙터 할당 없이 자동 연결
        private bool isJoystickActive = true;

        public void Awake()
        {
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn as Character);
            character = pawn as Character;
            
            skillAction001.action.Enable();
            skillAction002.action.Enable();
            
            if (joystick == null)
            {
                joystick = BattleOverlayCanvasController.Instance.joystick;
            }
        }

        public override void ProcessInputActions()
        {
            if (character == null || joystick == null)
            {
                return;
            }
            // 조이스틱 입력값으로 이동
            Vector2 moveDir = new Vector2(joystick.Horizontal, joystick.Vertical);
            this.moveDir = moveDir.normalized;

            if (!lockMovement && isJoystickActive)
            {
                character.Move(moveDir);
            }

            // 스킬 확인
            if (skillAction001.action.triggered)
            {
                character.ExecuteAttack(PawnAttackType.Skill1);
            }
            if (skillAction002.action.triggered)
            {
                character.ExecuteAttack(PawnAttackType.Skill2);
            }
        }

        public override void Update()
        {
            if (character is not null && isAutoAttack)
            {
                base.Update();
                character.ExecuteAttack(PawnAttackType.BasicAttack);
            }
        }

        public void SetJoystickActivate(bool value)
        {
            isJoystickActive = value;
        }
    }
} 