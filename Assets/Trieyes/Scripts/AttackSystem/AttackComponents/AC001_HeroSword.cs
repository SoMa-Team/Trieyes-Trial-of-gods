using AttackSystem;
using CharacterSystem;
using UnityEngine;

namespace AttackComponents
{
    /// <summary>
    /// 캐릭터 소드 공격
    /// 캐릭터 소드 공격은 캐릭터 소드 공격 로직을 만듭니다.
    /// 1. 캐릭터의 R_Weapon 게임 오브젝트를 가져옵니다.
    /// 2. R_Weapon에 콜라이더를 일정 시간동안 만들어줍니다.
    /// 3. 이 Component에 있는 attackAngle만큼 R_Weapon을 회전하며 공격합니다.
    /// 4. 공격이 끝나면 콜라이더를 삭제합니다.
    /// </summary>
    public class AC001_HeroSword : AttackComponent
    {
        public float attackDuration = 0.5f; // 공격 지속 시간

        private GameObject weaponGameObject; // 무기 오브젝트 참조

        private BoxCollider2D attackCollider;

        protected override void Start()
        {
            base.Start();

            // 3. attack 오브젝트에 콜라이더 추가 및 설정
            attackCollider = attack.GetComponent<BoxCollider2D>();
        }

        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);

            // 1. R_Weapon 오브젝트 찾기
            var pawnPrefab = attack.attacker.PawnPrefab;
            weaponGameObject = pawnPrefab.transform.Find("UnitRoot/Root/BodySet/P_Body/ArmSet/ArmR/P_RArm/P_Weapon/R_Weapon")?.gameObject;

            if (weaponGameObject == null)
            {
                Debug.LogError("R_Weapon을 찾지 못했습니다!");
                return;
            }

            // 2. attack 오브젝트를 R_Weapon의 자식으로 설정
            attack.transform.SetParent(weaponGameObject.transform);
            attack.transform.localPosition = Vector3.zero;
            attack.transform.localRotation = Quaternion.identity;

            attackCollider.offset = new Vector2(0, 0.3f);
            attackCollider.size = new Vector2(0.16f, 0.56f);
            attackCollider.isTrigger = true;
            attackCollider.enabled = true;
            attack.attackCollider = attackCollider;

            // 4. 애니메이션 트리거
            // attack.attacker.ChangeAnimationState("ATTACK");
        }

        protected override void Update()
        {
            base.Update();
            attack.transform.position = attack.attacker.transform.position;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (weaponGameObject != null)
            {
                foreach (var collider in weaponGameObject.GetComponents<BoxCollider2D>())
                {
                    Destroy(collider);
                }
            }
        }
    }
}