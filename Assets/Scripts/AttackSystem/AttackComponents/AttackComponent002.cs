using Utils; // For EventType
using UnityEngine; // For Debug.Log
using AttackSystem;
using Stats;

namespace AttackComponents
{
    public class AttackComponent002 : AttackComponent
    {
        // ===== [기능 1] 공격 실행 및 관련 메소드 =====
        public override void Execute(Attack attack)
        {
            // StatSystem을 활용한 효과 적용
            if (attack.attacker != null)
            {
                var buff = new StatModifier(20, BuffOperationType.Multiplicative, false, 3f);
                attack.attacker.statSheet[StatType.Defense].AddBuff(buff);
                Debug.Log($"AttackComponent002: 대상에게 방어력 증가 효과를 부여합니다.");
            }
        }

        // ===== [기능 2] 이벤트 처리 =====
        protected override void HandleOnDeath(object param)
        {
            if (param is CharacterSystem.Pawn deadPawn && deadPawn.gameObject != null)
            {
                Debug.Log($"AttackComponent002: {deadPawn.gameObject.name} 사망 이벤트 수신! 임시 방어 버프를 얻습니다.");
                // StatSystem의 버프 시스템 활용
                var buff = new StatModifier(15, BuffOperationType.Multiplicative, false, 5f);
                deadPawn.statSheet[StatType.Defense].AddBuff(buff);
            }
        }

        protected override void HandleOnBattleEnd(object param)
        {
            Debug.Log($"AttackComponent002: 전투 종료! 특정 방어 패턴 비활성화.");
        }
    }
} 