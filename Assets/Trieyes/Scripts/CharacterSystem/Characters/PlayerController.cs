using System;
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
        public InputActionReference skillAction001;
        public InputActionReference skillAction002;

        public Joystick joystick; // 인스펙터 할당 없이 자동 연결

        public void Awake()
        {
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            skillAction001.action.Enable();
            skillAction002.action.Enable();
            
            if (joystick == null)
            {
                var canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    var found = canvas.GetComponentInChildren<Joystick>(true);
                    joystick = found as Joystick;
                }
            }
        }

        public override void ProcessInputActions()
        {
            if (owner == null || joystick == null)
            {
                return;
            }
            // StatSheet에서 최신 MoveSpeed를 반영
            owner.moveSpeed = owner.GetStatValue(Stats.StatType.MoveSpeed);
            // 조이스틱 입력값으로 이동
            Vector2 moveDir = new Vector2(joystick.Horizontal, joystick.Vertical);
            this.moveDir = moveDir.normalized;
            
            owner.Move(moveDir);

            // 스킬 확인
            if (skillAction001.action.triggered)
            {
                owner.ExecuteSkillAttack(owner.skillAttack001);
            }
            if (skillAction002.action.triggered)
            {
                owner.ExecuteSkillAttack(owner.skillAttack002);
            }
        }

        private void Update()
        {
            owner.PerformAutoAttack(); // 자동 공격
        }
    }
} 