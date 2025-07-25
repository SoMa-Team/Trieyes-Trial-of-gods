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
    public class E001_Test : Enemy
    {        
        // ===== [기능 2] 초기화 =====
        protected override void Start()
        {
            base.Start();
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            dropGold = 10;
            // TODO: AttackComponent 할당
            ////Debug.Log("Enemy001 Activated.");

            // 이런 느낌으로 각 적마다 커스터마이징 
            // boxCollider = Collider as BoxCollider2D;
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
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 사망 시 골드 드랍 처리
        /// </summary>
        /// <param name="param">이벤트 파라미터</param>
        protected override void OnSelfDeath(AttackResult result)
        {
            base.OnSelfDeath(result);
        }
    }
} 
