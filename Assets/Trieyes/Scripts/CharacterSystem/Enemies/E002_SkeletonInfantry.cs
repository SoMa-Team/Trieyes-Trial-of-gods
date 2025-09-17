using AttackSystem;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 사망 시 골드를 드랍하는 기본 적 캐릭터
    /// </summary>
    public class E002_SkeletonInfantry : Enemy
    {        
        [Header("Sprite Renderer & Material")]
        [SerializeField] private SpriteRenderer sr;

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        protected override void Start()
        {
            base.Start();
            allIn1SpriteShaderHandler.SetObject(sr.material);
        }
        
        public override void Activate()
        {
            base.Activate();
            dropGold = 1;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public override void Deactivate()
        {
            dropGold = 1;
            base.Deactivate();
        }

        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            var result = base.OnEvent(eventType, param);

            if (result)
            {
                if (eventType == Utils.EventType.OnDeath)
                {
                    EnemyFactory.Instance.Deactivate(this);
                }
                return true;
            }
            return result;
        }
    }
} 
