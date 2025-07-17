using AttackSystem;
using UnityEngine;
using System;
using System.Collections;
using CharacterSystem;

namespace AttackComponents
{
    public enum DOTType
    {
        Fire,
        Ice,
        Lightning,
        Poison,
        Bleed,
    }

    public enum DOTCollisionType
    {
        Individual, // 모기처럼 한명한테 붙어서 도트 주는 놈
        AreaRect, // 네모 장판 범위 내 모든 적에게 도트 주는 놈
        AreaCircle, // 원형 장판 범위 내 모든 적에게 도트 주는 놈
    }

    /// <summary>
    /// 도트 효과 적용
    /// 공격에 맞은 적에게 지속적으로 화상데미지(**도트**)를 입힙니다
    /// </summary>
    public class AC100_AOE  : AttackComponent
    {   
        // 도트 타입 ENUM
        public DOTType dotType;

        // 도트 콜리전 타입 (개별 Pawn에 적용되는지, Box Collider로 장판처럼 적용되는지)
        public DOTCollisionType dotCollisionType;
        public int dotDamage = 10;
        public float dotDuration = 10f;

        public float currentDotDuration = 0f;
        public float dotInterval = 1f;

        // 도트 콜리전 타입이 Individual일 때 사용되는 값
        public Pawn target;

        // 도트 콜리전 타입이 AreaRect일 때 사용되는 값
        public float dotWidth = 1f;
        public float dotHeight = 1f;

        // 도트 콜리전 타입이 AreaCircle일 때 사용되는 값
        public float dotRadius = 1f;
        public float dotAngle = 180f;
        public int dotSegments = 8;

        public override void Activate(Attack attack, Vector2 direction)
        {
            if (attack.target != null)
            {
                dotCollisionType = DOTCollisionType.Individual;
                target = attack.target;
            }
            else
            {
                dotCollisionType = DOTCollisionType.AreaRect;
            }
        }

        private void DOTHandlerByCollisionType(DOTCollisionType dotCollisionType)
        {
            switch (dotCollisionType)
            {
                case DOTCollisionType.Individual:
                    DOTHandlerByIndividual();
                    break;
                case DOTCollisionType.AreaRect:
                    DOTHandlerByAreaRect();
                    break;
                case DOTCollisionType.AreaCircle:
                    DOTHandlerByAreaCircle();
                    break;
                default:
                    break;
            }
        }

        private void DOTHandlerByIndividual()
        {
            AttackResult result = new AttackResult();
            result.attacker = attack.attacker;
            result.totalDamage = dotDamage;

            target.ApplyDamage(result);

            if (target.isDead)
            {
                AttackFactory.Instance.Deactivate(attack);
            }
        }

        private void DOTHandlerByAreaRect()
        {
            throw new NotImplementedException();
        }

        private void DOTHandlerByAreaCircle()
        {
            throw new NotImplementedException();
        }


        // DOT 클래스의 Update 함수는 dotInterval 마다 호출되며, dotDuration 만큼 지속됩니다.
        protected override void Update()
        {
            if (target == null || target.isDead || dotDuration <= 0f)
            {
                AttackFactory.Instance.Deactivate(attack);
                return;
            }

            base.Update();
            if (currentDotDuration < dotDuration && currentDotDuration >= dotInterval)
            {
                DOTHandlerByCollisionType(dotCollisionType);
                currentDotDuration = 0f;
                dotDuration -= dotInterval;
            }
            else
            {
                currentDotDuration += Time.deltaTime;
            }
        }
    }
}