using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;
using BattleSystem;

namespace CharacterSystem
{
    public class Character : Pawn
    {
        // ===== [필드] =====
        
        // Pawn의 추상 멤버 구현
        
        // ===== [Unity 생명주기] =====
        protected override void Start()
        {
            base.Start();
            
            // Collision Layer를 Character로 설정
            gameObject.layer = LayerMask.NameToLayer("Character");
            
            RegisterAcceptedEvents(
                Utils.EventType.OnLevelUp
            );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }
        
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Boundary"))
            {
                // 경계선 밖으로 나가려 하면 이전 위치로 되돌림
                transform.position = transform.position;
                Debug.Log("Character001 OnTriggerExit2D");
            }
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

            var capsuleCollider = Collider as CapsuleCollider2D;
            capsuleCollider.isTrigger = true;
            this.transform.position = Vector3.zero;

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
        public override bool OnEvent(Utils.EventType eventType, object param)
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
                    return true;
                    break;
                case Utils.EventType.OnDeath:
                    // 죽고 나서 할 것
                    if(BattleStage.now.mainCharacter == this)
                    {
                        BattleStage.now.OnPlayerDeath();
                    }

                    return true;
                    break;
            }
            return false;
        }
    }
}