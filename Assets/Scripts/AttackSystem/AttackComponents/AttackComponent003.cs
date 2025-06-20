using UnityEngine;
using AttackSystem;
using CharacterSystem;
using Utils;
using System.Linq;

namespace AttackComponents
{
    /// <summary>
    /// ID: 003 - 기본적으로 검을 휘두르는(근접) 형태의 AttackComponent
    /// </summary>
    public class AttackComponent003 : AttackComponent
    {
        [Header("검 휘두르기 설정")]
        [SerializeField] private float swingRange = 1.5f; // 휘두르기 범위
        [SerializeField] private int baseDamage = 15; // 기본 데미지
        [SerializeField] private LayerMask targetLayer;

        public override void Execute(Attack attack)
        {
            // 공격자의 위치와 방향 기준으로 범위 내 적 탐색
            if (attack.attacker == null) return;
            Vector2 origin = attack.attacker.transform.position;
            Vector2 direction = attack.attacker.transform.right; // 오른쪽이 기본 방향

            // 원형 범위 내 적 탐색
            Collider2D[] hits = Physics2D.OverlapCircleAll(origin + direction * swingRange * 0.5f, swingRange * 0.5f, targetLayer);
            foreach (var hit in hits)
            {
                Pawn targetPawn = hit.GetComponent<Pawn>();
                if (targetPawn != null && targetPawn != attack.attacker)
                {
                    // 데미지 적용 (OnDamaged 호출)
                    targetPawn.OnEvent(Utils.EventType.OnDamaged, new Pawn.AttackEventData(attack.attacker, targetPawn));
                    Debug.Log($"<color=orange>[AttackComponent003] {attack.attacker.gameObject.name}가 {targetPawn.gameObject.name}에게 검 휘두르기({baseDamage}) 데미지!");
                }
            }
        }

        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param); // 부모 클래스의 OnEvent 호출

            if (eventType == Utils.EventType.OnBattleStart)
            {
                Debug.Log("[AttackComponent003] 전투 시작: 검 휘두르기 준비 완료");
            }
        }

        // 디버그용: 에디터에서 범위 시각화
        private void OnDrawGizmosSelected()
        {
            if (owner != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(owner.transform.position + owner.transform.right * swingRange * 0.5f, swingRange * 0.5f);
            }
        }
    }
} 