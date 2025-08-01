using AttackSystem;
using CharacterSystem;
using UnityEngine;
using System.Collections.Generic;

namespace AttackComponents
{
    /// <summary>
    /// 속성 부여 스킬을 사용할 때마다 해당 라운드가 끝날 때까지 자신의 주위를 공전하는 속성 별을 생성하는 컴포넌트
    /// AC108 매니저를 사용하여 공전 객체를 관리합니다.
    /// </summary>
    public class RAC009_OrbitingStarGenerator : AttackComponent
    {
        public AttackData orbitingManagerAttackData;
        public AttackData orbitingStarAttackData; // AC107용 AttackData
        private Character001_Hero hero;
        
        public GameObject NoneVFX;
        public GameObject FireVFX;
        public GameObject IceVFX;
        public GameObject LightningVFX;
        public GameObject LightVFX;

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            hero = attack.attacker as Character001_Hero;
            
            if (hero != null && hero.orbitingManager == null)
            {
                InitializeOrbitingManager();
            }
            
            if (orbitingStarAttackData == null)
            {
                return;
            }
        }

        public override void OnLockActivate()
        {
            base.OnLockActivate();

            if (hero != null)
            {     
                // 속성별 VFX 프리팹 선택
                GameObject vfxPrefab = GetVFXPrefabByElement(hero.weaponElementState);
                
                // AC108 매니저를 통해 공전 객체 추가 (Pawn 타입 전달)
                hero.orbitingManager.AddOrbitingObject(orbitingStarAttackData, hero, vfxPrefab);
            }
        }

        /// <summary>
        /// AC108 매니저를 초기화합니다. (AttackComponentFactory 사용)
        /// </summary>
        private void InitializeOrbitingManager()
        {
            // AttackComponentFactory를 통해 AC108 생성
            Attack orbitingManager = AttackFactory.Instance.Create(
                orbitingManagerAttackData,
                hero,
                null, // Attack 객체는 나중에 설정
                Vector2.zero
            );
            
            if (orbitingManager != null)
            {
                // setParent로는 붙지 않고, 트랜스폼만 계속 연동
                orbitingManager.transform.position = hero.transform.position;
                
                var orbitingManagerComponent = orbitingManager.components[0] as AC108_OrbitingManager;
                
                // 매니저 설정
                if (orbitingManagerComponent != null)
                {
                    orbitingManagerComponent.SetOrbitTarget(hero.transform);
                    orbitingManagerComponent.SetOrbitParameters(2f, 90f, OrbitDirection.Clockwise);
                }
                
                // hero.orbitingManager에 할당
                hero.orbitingManager = orbitingManagerComponent;
                
                Debug.Log("[RAC009] AC108 매니저 초기화 완료! (AttackComponentFactory 사용)");
            }
        }

        /// <summary>
        /// 속성에 따른 VFX 프리팹을 반환합니다.
        /// </summary>
        /// <param name="elementState">속성 상태</param>
        /// <returns>VFX 프리팹</returns>
        private GameObject GetVFXPrefabByElement(HeroWeaponElementState elementState)
        {
            switch (elementState)
            {
                case HeroWeaponElementState.Fire:
                    return FireVFX;
                case HeroWeaponElementState.Ice:
                    return IceVFX;
                case HeroWeaponElementState.Lightning:
                    return LightningVFX;
                case HeroWeaponElementState.Light:
                    return LightVFX;
                default:
                    return NoneVFX;
            }
        }
    }
} 