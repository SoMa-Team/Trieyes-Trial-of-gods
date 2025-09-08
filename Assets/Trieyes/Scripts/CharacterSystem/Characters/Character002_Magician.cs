using UnityEngine;
using System.Linq;
using AttackSystem;
using Stats;
using AttackComponents;

namespace CharacterSystem
{
    
    public class Character002_Magician : Character
    {
        // ===== [필드] =====
        [HideInInspector] public int killedDuringSkill001 = 0;
        [HideInInspector] public int killedDuringSkill002 = 0;

        [HideInInspector] public AttackData _basicAttack;
        
        // Pawn의 추상 멤버 구현
        
        // ===== [Unity 생명주기] =====
        protected override void Start()
        {
            base.Start();
            pawnName = "마법사";
            spawnID = 2;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Update()
        {
            base.Update();
        }

        // ===== [커스텀 메서드] =====
        public override void Activate()
        {
            base.Activate();
            _basicAttack = basicAttack;
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);

            return false;
        }
    }
}
