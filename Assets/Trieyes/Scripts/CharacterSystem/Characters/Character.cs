using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;

namespace CharacterSystem
{
    public class Character : Pawn
    {
        // ===== [필드] =====
        
        // Pawn의 추상 멤버 구현
        
        // ===== [Unity 생명주기] =====
        protected override void Awake()
        {
            base.Awake();
            
            // Collision Layer를 Character로 설정
            gameObject.layer = LayerMask.NameToLayer("Character");
            
            // PlayerController를 동적으로 붙이거나, 인스펙터에서 할당
            Controller = GetComponent<Controller>();
            if (Controller == null)
            {
                throw new System.Exception("PlayerController not found on " + gameObject.name);
            }
            Controller.Activate(this);
        }

        protected override void Start()
        {
            base.Start();
            
            // Character001만 받을 특별한 이벤트들 등록
            RegisterAcceptedEvents(
                Utils.EventType.OnLevelUp
            );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Update()
        {
            base.Update();
            Controller?.ProcessInputActions();
        }

        // ===== [커스텀 메서드] =====
        public override void Activate()
        {
            base.Activate();
            transform.position = Vector3.zero;
            //Debug.Log("Character001 Activated.");
        }

        public override void Deactivate()
        {
            base.Deactivate();
            //Debug.Log("Character001 Deactivated.");
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            // 이벤트 필터링: Character001이 받지 않는 이벤트는 무시
            // if (!IsEventAccepted(eventType))
            // {
            //     Debug.Log($"<color=gray>[EVENT_FILTER] {gameObject.name} (Character001) ignoring event: {eventType} (not in accepted events: {string.Join(", ", GetAcceptedEvents())})</color>");
            //     return;
            // }

            //Debug.Log($"<color=green>[EVENT_FILTER] {gameObject.name} (Character001) accepting event: {eventType}</color>");

            // 부모의 이벤트 전파 로직 호출 (필터링 적용됨)
            base.OnEvent(eventType, param);

            // Character001 고유의 이벤트 처리
            switch (eventType)
            {
                case Utils.EventType.OnLevelUp:
                    //Debug.Log($"<color=yellow>{gameObject.name} (Character001) gained a level!</color>");
                    break;
                case Utils.EventType.OnDeath:
                    // Character001의 사망 이벤트는 base.OnEvent에서 이미 처리됨
                    // 여기서는 고유한 추가 로직만 수행
                    //Debug.Log($"<color=yellow>{gameObject.name} (Character001) is performing its unique death action: Game Over!</color>");
                    break;
            }
        }
    }
}