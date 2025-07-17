using AttackSystem;
using CharacterSystem;

namespace BattleSystem
{
    public class DamageProcessor
    {
        /// <summary>
        /// 공격이 타겟에 맞을 경우 처리합니다.
        /// Pawn, Attack에 이벤트를 전파합니다.
        /// </summary>
        /// <param name="attack">피격한 Attack</param>
        /// <param name="targetPawn">피격당한 Pawn</param>
        public static void ProcessHit(Attack attack, Pawn targetPawn)
        {
            Pawn attacker = attack.attacker;
            
            var result = AttackResult.Create(attack, targetPawn);
            
            // OnAttackHit, OnDamagedHit
            triggerAttackHitEvent(result);

            if (result.isEvaded)
            {
                // OnEvaded, OnAttackMissed
                triggerEvadeEvent(result);
                return;
            }
            
            // OnAttack, OnDamaged
            triggerAttackEvent(result);
            if (result.isCritical)
            {
                triggerCriticalAttackEvent(result);
            }
        }

        private static void triggerAttackHitEvent(AttackResult result)
        {
            //Debug.Log($"<color=yellow>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttackHit -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
            //Debug.Log($"<color=red>[EVENT] {gameObject.name} -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name}) OnDamageHit <- Attacker {attacker.gameObject.name} ({attacker.GetType().Name})</color>");
            
            // 1. 공격자의 OnAttackHit 이벤트 (유물, 카드 순회)
            result.attack.OnEvent(Utils.EventType.OnAttackHit, result);
            result.attacker.OnEvent(Utils.EventType.OnAttackHit, result);

            // 2. 피격자의 OnDamageHit 이벤트 (회피 판정)
            result.target.OnEvent(Utils.EventType.OnDamageHit, result);
        }

        private static void triggerEvadeEvent(AttackResult result)
        {
            //Debug.Log($"<color=cyan>[EVENT] {gameObject.name} -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name}) OnEvaded (SUCCESS)</color>");
            //Debug.Log($"<color=cyan>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttackMiss -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
            // 회피 성공: OnEvaded (피격자) + OnAttackMiss (공격자)
                    
            result.attack.OnEvent(Utils.EventType.OnAttackMiss, result);
            result.attacker.OnEvent(Utils.EventType.OnAttackMiss, result);
            result.target.OnEvent(Utils.EventType.OnEvaded, result);
        }

        private static void triggerAttackEvent(AttackResult result)
        {
            //Debug.Log($"<color=green>[EVENT] {gameObject.name} -> Attacker {attacker.gameObject.name} ({attacker.GetType().Name}) OnAttack -> Target {targetPawn.gameObject.name} ({targetPawn.GetType().Name})</color>");
            // 회피 실패: OnAttack (공격자) - 데미지 계산 및 OnDamaged 호출
            result.attack.OnEvent(Utils.EventType.OnAttack, result);
            result.attacker.OnEvent(Utils.EventType.OnAttack, result);
            result.target.OnEvent(Utils.EventType.OnDamaged, result);
        }
        
        private static void triggerCriticalAttackEvent(AttackResult result)
        {
            result.attack.OnEvent(Utils.EventType.OnCriticalAttack, result);
            result.attacker.OnEvent(Utils.EventType.OnCriticalAttack, result);
        }
    }
}