using AttackSystem;
using UnityEngine;
using Utils;
using System.Linq;
using BattleSystem;
using Stats;

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
            // Enemy001 고유 정리 로직
            dropGold = 10; // 기본값으로 초기화
            
            base.Deactivate();
            ////Debug.Log("Enemy001 Deactivated.");
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
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
