using System.Collections.Generic;
using CharacterSystem;
using Utils;
using UnityEngine;

namespace AttackSystem
{
    /// <summary>
    /// 게임 내 공격 행위를 정의하는 클래스입니다.
    /// 이 클래스는 IEventHandler를 구현하여 자체적으로 이벤트를 처리하고 발동시킬 수 있습니다.
    /// </summary>
    public class Attack : IEventHandler
    {
        // ===== 생성자 및 공용 =====
        public Attack(AttackData data)
        {
            attackData = data;
            // attackComponents 초기화 로직 (외부에서 주입될 수도 있음)
        }

        // ===== [기능 1] 공격 데이터 및 컴포넌트 관리 =====
        public AttackData attackData;
        public Pawn attacker;
        public List<AttackComponent> components = new List<AttackComponent>();

        public void Activate()
        {
            Debug.Log("Attack Activated!");
            // 공격 활성화 로직
        }

        public void Deactivate()
        {
            Debug.Log("Attack Deactivated!");
            // 공격 비활성화 로직
        }

        public void Execute()
        {
            foreach (var comp in components)
                comp.Execute(this);
        }
        
        // ===== [기능 2] 이벤트 처리 =====
        public virtual void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnBattleEnd:
                Debug.Log($"Attack: 전투 종료 이벤트 수신! 공격 비활성화.");
                Deactivate();
                    break;
                case Utils.EventType.OnDeath:
                if (param is CharacterSystem.Pawn deadPawn)
                {
                    Debug.Log($"Attack: {deadPawn.gameObject.name} 사망 이벤트 수신. 특정 공격 버프 제거.");
                    // 사망한 대상에 따른 공격 관련 로직 구현
                }
                    break;
            // 다른 이벤트 처리 로직 추가
        }
        }
    }
} 