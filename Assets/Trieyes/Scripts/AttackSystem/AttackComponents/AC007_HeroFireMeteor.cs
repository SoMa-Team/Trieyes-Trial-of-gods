using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;
using BattleSystem;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC002)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// 천상 : 이동속도와 사거리가 증가합니다. 방어력이 감소합니다. AC1001_BUFF 버프를 줍니다.
    /// </summary>
    public class AC007_HeroFireMeteor : AttackComponent
    {
        private const int FALL_ID = 14;

        /// <summary>
        /// 파이어 메테오 공격 활성화
        /// AC102_FALL 소환하고 바로 종료
        /// </summary>
        /// <param name="attack"></param>
        /// <param name="direction"></param>
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);

            // AC102_FALL 소환
            var fallAttack = AttackFactory.Instance.ClonePrefab(FALL_ID);
            BattleStage.now.AttachAttack(fallAttack);
            fallAttack.target = attack.target;
            var fallComponent = fallAttack.components[0] as AC102_FALL;
            fallComponent.fallXYOffset = new Vector2(1, 1);
            fallAttack.Activate(attack.attacker, direction);

            AttackFactory.Instance.Deactivate(attack);
        }
    }
}