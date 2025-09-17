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
    public class E004_Archer : Enemy
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
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public override void Deactivate()
        {
            dropGold = 10; // 기본값으로 초기화
            
            base.Deactivate();
        }

        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);
            return true;
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
