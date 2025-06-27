using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;

namespace CharacterSystem
{
    public class Character001 : Pawn
    {
        // ===== [기능 1] 고유 필드 =====
        public int experience = 0;

        // ===== [기능 2] 초기화 및 스탯 =====
        protected override void Awake()
        {
            base.Awake();
            this.gold = 1000;
            
            // PlayerController를 동적으로 붙이거나, 인스펙터에서 할당
            playerController = GetComponent<PlayerController>();
            if (playerController == null)
            {
                throw new System.Exception("PlayerController not found on " + gameObject.name);
            }
            playerController.Initialize(this);
        }

        protected override void Start()
        {
            base.Start();
            
            // Character001만 받을 특별한 이벤트들 등록
            RegisterAcceptedEvents(
                Utils.EventType.OnLevelUp
            );
            
            // Character001의 카드들이 받을 특별한 이벤트들 등록
            RegisterCardAcceptedEvents(
                Utils.EventType.OnLevelUp
            );
            
            // Character001의 유물들이 받을 특별한 이벤트들 등록
            RegisterRelicAcceptedEvents(
                Utils.EventType.OnLevelUp
            );
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        // ===== [기능 3] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            // 이벤트 필터링: Character001이 받지 않는 이벤트는 무시
            if (!IsEventAccepted(eventType))
            {
                Debug.Log($"<color=gray>[EVENT_FILTER] {gameObject.name} (Character001) ignoring event: {eventType} (not in accepted events: {string.Join(", ", GetAcceptedEvents())})</color>");
                return;
            }

            Debug.Log($"<color=green>[EVENT_FILTER] {gameObject.name} (Character001) accepting event: {eventType}</color>");

            // 부모의 이벤트 전파 로직 호출 (필터링 적용됨)
            base.OnEvent(eventType, param);

            // Character001 고유의 이벤트 처리
            switch (eventType)
            {
                case Utils.EventType.OnLevelUp:
                    Debug.Log($"<color=yellow>{gameObject.name} (Character001) gained a level!</color>");
                    break;
                case Utils.EventType.OnDeath:
                    // Character001의 사망 이벤트는 base.OnEvent에서 이미 처리됨
                    // 여기서는 고유한 추가 로직만 수행
                    if (param as Pawn == this)
                    {
                        Debug.Log($"<color=yellow>{gameObject.name} (Character001) is performing its unique death action: Game Over!</color>");
                    }
                    break;
            }
        }
        
        // ===== 활성화/비활성화 =====
        public override void Activate()
        {
            base.Activate();
            Debug.Log("Character001 Activated.");
        }

        
        public override void Deactivate()
        {
            // Character001 고유 정리 로직
            experience = 0;
            gold = 0;
            
            base.Deactivate();
            Debug.Log("Character001 Deactivated.");
        }

        public override void Update()
        {
            base.Update();
            playerController?.ProcessInput();
        }
    }
} 