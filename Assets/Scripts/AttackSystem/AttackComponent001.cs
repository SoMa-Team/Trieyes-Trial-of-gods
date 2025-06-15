using Core; // For EventType
using UnityEngine; // For Debug.Log

namespace AttackSystem
{
    public class AttackComponent001 : AttackComponent
    {
        public override void Execute(Attack attack)
        {
            // 공격 효과 구현 (예: 기본 데미지 적용)
            Debug.Log($"AttackComponent001: {attack.attackData.damage} 데미지를 가합니다.");
            // 실제 데미지 적용 로직은 Attack 클래스나 대상 Pawn에서 처리될 수 있습니다.
        }

        /// <summary>
        /// 이 AttackComponent가 특정 이벤트에 반응할 때 호출됩니다.
        /// </summary>
        /// <param name="eventType">발동된 이벤트 타입</param>
        /// <param name="param">이벤트 매개변수</param>
        public override void OnEvent(Core.EventType eventType, object param)
        {
            if (eventType == Core.EventType.OnDeath) // 사망 이벤트에 반응
            {
                if (param is CharacterSystem.Pawn deadPawn)
                {
                    Debug.Log($"AttackComponent001: {deadPawn.gameObject.name} 사망 이벤트 수신! 임시 공격 버프를 얻습니다.");
                    // 예시: 공격력 일시 증가 로직
                    // 이 AttackComponent가 속한 Attack 인스턴스의 공격력을 변경할 수 있습니다.
                }
            }
            else if (eventType == Core.EventType.OnBattleStart)
            {
                Debug.Log($"AttackComponent001: 전투 시작! 특정 공격 패턴 활성화.");
            }
            // 다른 이벤트에 대한 로직을 여기에 추가
        }
    }
}