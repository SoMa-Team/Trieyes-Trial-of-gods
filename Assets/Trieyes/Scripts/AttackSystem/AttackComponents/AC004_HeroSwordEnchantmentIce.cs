using AttackSystem;
using CharacterSystem;
using Stats;
using UnityEngine;
using System.Threading;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 능력 부여 강화
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 7초 동안 검에 무작위 속성을 부여하고, 기본 공격(AC002)에 다음의 추가효과가 적용되고, 추가 피해를 입힙니다.
    /// - 불꽃 : 공격에 맞은 적에게 지속적으로 화상데미지(**도트**)를 입힙니다
    /// - 얼음 : 공격에 맞은 적들을 둔화 시킵니다.
    /// - 번개 : 공격에 맞은 대상 주변 적들이 연쇄적인 번개(**쓰리쿠션 데미지-관통 개수에 비례**) 피해를 입습니다.
    /// - 천상 : 이동속도와 사거리가 증가합니다. 방어력이 감소합니다. (유물 껴야만 나옴)
    /// </summary>
    public class AC004_HeroSwordEnchantmentIce  : AttackComponent
    {
        public float freezeRate = 15f; // 이동속도 둔화 퍼센트
        public float freezeDuration = 5f; // 둔화 지속 시간        

        public override void Activate(Attack attack, Vector2 direction)
        {
        }

        protected override void Update()
        {
            base.Update();
            attack.transform.position = attack.attacker.transform.position;
            attack.transform.rotation = Quaternion.Euler(0, 0, 0);

            freezeDuration -= Time.deltaTime;
            if (freezeDuration <= 0f)
            {
                AttackFactory.Instance.Deactivate(attack);
            }
        }
    }
}