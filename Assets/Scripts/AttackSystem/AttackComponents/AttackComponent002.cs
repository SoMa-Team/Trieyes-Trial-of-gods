using Utils; // For EventType
using UnityEngine; // For Debug.Log
using AttackSystem;

namespace AttackComponents
{
    public class AttackComponent002 : AttackComponent
    {
        // ===== [기능 1] 공격 실행 및 관련 메소드 =====
        public override void Execute(Attack attack)
        {
            // 공격 효과 구현 (예: 특정 상태 이상 적용)
            Debug.Log($"AttackComponent002: 대상에게 특정 효과를 부여합니다.");
            // 실제 효과 적용 로직은 Attack 클래스나 대상 Pawn에서 처리될 수 있습니다.
        }

        // ===== [기능 2] 이벤트 처리 =====
        /// <summary>
        /// 이 AttackComponent가 특정 이벤트에 반응할 때 호출됩니다.
        /// </summary>
        /// <param name="eventType">발동된 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            switch (eventType)
            {
                case Utils.EventType.OnDeath:
                    if (param is CharacterSystem.Pawn deadPawn)
                    {
                        Debug.Log($"AttackComponent002: {deadPawn.gameObject.name} 사망 이벤트 수신! 임시 방어 버프를 얻습니다.");
                        // 예시: 방어력 일시 증가 로직
                    }
                    break;
                case Utils.EventType.OnBattleEnd:
                    Debug.Log($"AttackComponent002: 전투 종료! 특정 방어 패턴 비활성화.");
                    break;
                // 다른 이벤트에 대한 로직을 여기에 추가
            }
        }
    }
} 