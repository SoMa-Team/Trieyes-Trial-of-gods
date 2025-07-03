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

        // ===== [Unity 생명주기] =====
        // private void OnEnable() { }
        // private void OnDisable() { }
        // private void Update() { }

        private void Awake()
        {
            if (joystick == null)
            {
                // "Canvas"라는 이름의 오브젝트를 먼저 찾음
                var canvas = GameObject.Find("Canvas");
                if (canvas != null)
                {
                    var found = canvas.GetComponentInChildren<Joystick>(true);
                    joystick = found as FixedJoystick;
                }
            }
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 입력을 처리하고 Pawn에 명령을 전달합니다.
        /// </summary>
        public override void ProcessInputActions()
        {
            if (owner == null || joystick == null)
            {
                return;
            }
            // 조이스틱 입력값으로 이동
            Vector2 moveDir = new Vector2(joystick.Horizontal, joystick.Vertical);
            owner.Move(moveDir);
            // 공격 버튼 연동 시 moveDir 방향으로 공격 등 추가 가능
        }
    }
} 