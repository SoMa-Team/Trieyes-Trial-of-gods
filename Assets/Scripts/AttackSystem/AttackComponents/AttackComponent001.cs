using UnityEngine; // For Debug.Log
using AttackSystem;
using Stats;

namespace AttackComponents
{
    public class AttackComponent001 : AttackComponent
    {
        // ===== [기능 1] 공격 실행 및 관련 메소드 =====
        public override void Execute(Attack attack)
        {
            // StatSystem을 활용한 데미지 계산
            int finalDamage = attack.attackData.statSheet[StatType.AttackPower].Value;
            Debug.Log($"AttackComponent001: {finalDamage} 데미지를 가합니다.");
            // 실제 데미지 적용 로직은 Attack 클래스나 대상 Pawn에서 처리될 수 있습니다.
        }

        // ===== [기능 2] 이벤트 처리 =====
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param); // 부모 클래스의 OnEvent 호출

            if (eventType == Utils.EventType.OnDeath)
            {
                if (param is CharacterSystem.Pawn deadPawn && deadPawn.gameObject != null)
                {
                    Debug.Log($"AttackComponent001: {deadPawn.gameObject.name} 사망 이벤트 수신! 임시 공격 버프를 얻습니다.");
                    // StatSystem의 버프 시스템 활용
                    var buff = new StatModifier(10, BuffOperationType.Additive, false, 5f);
                    deadPawn.statSheet[StatType.AttackPower].AddBuff(buff);
                }
            }
            
            if (eventType == Utils.EventType.OnBattleStart)
            {
                Debug.Log($"AttackComponent001: 전투 시작! 특정 공격 패턴 활성화.");
            }
        }
    }
}