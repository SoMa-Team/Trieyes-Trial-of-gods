using System;
using UnityEngine;
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

        public Joystick joystick; // 인스펙터 할당 없이 자동 연결

        public void Awake()
        {
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
            // 공격 버튼 연동 시 moveDir 방향으로 공격 등 추가 가능
        }

        private void Update()
        {
            owner.PerformAutoAttack(); // 자동 공격
        }

        public override void Activate(Pawn pawn)
        {
            base.Activate(pawn);
            
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
    }
} 