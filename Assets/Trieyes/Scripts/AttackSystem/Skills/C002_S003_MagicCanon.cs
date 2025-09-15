using AttackSystem;
using CharacterSystem;
using UnityEngine;
using Stats;

namespace AttackComponents
{
    /// <summary>
    /// 현재 위치에 [10]초 동안 지속하는 공명 포탑을 생성합니다. 
    /// 공명포탑은 어떠한 마법 공격이 닿았을 경우 전방 [<2>] Radius에 [100]% 광역 마법 공격을 발사합니다.
    /// </summary>
    public class C002_S003_MagicCanon : AttackComponent
    {
        private Character002_Magician character;

        private Enemy hitEnemy;

        public AttackData AOEAttackData;
        // FSM 상태 관리
        private MagicCanonState magicCanonState = MagicCanonState.None;

        public float magicCanonRadius = 2f;

        public GameObject magicCanon;
        public GameObject magicCanonAOEParticle;

        private Vector3 magicCanonPosition;
        
        private float magicCanonTimer = 0f;
        private float magicCanonDuration = 10f;

        // 마법 포탑 상태 열거형
        private enum MagicCanonState
        {
            None,
            Install,
            Active,
            AOE,
            Finished
        }
        
        public override void Activate(Attack attack, Vector2 direction)
        {
            base.Activate(attack, direction);
            
            character = attack.attacker as Character002_Magician;

            if (character is null)
            {
                Debug.LogError("[S003] Character002_Magician 컴포넌트를 찾을 수 없습니다!");
                return;
            }
            
            magicCanonState = MagicCanonState.Install;
            magicCanonPosition = character.transform.position;
            magicCanonTimer = 0f;
        }

        protected override void Update()
        {
            base.Update();
            
            // Lock 상태일 때는 Update 실행하지 않음
            if (isLocked) return;
            
            // 마법 포탑 상태 처리
            ProcessMagicCanonState();
        }

        public override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);

            // 마법 공격이 포탑에 닿았을 때 AOE 공격 발사
            if (other.CompareTag("Attack") && magicCanonState == MagicCanonState.Active)
            {
                magicCanonState = MagicCanonState.AOE;
                ExecuteAOE();
            }
        }

        private void ProcessMagicCanonState()
        {
            switch (magicCanonState)
            {
                case MagicCanonState.None:
                    break;

                case MagicCanonState.Install:
                    // 마법 포탑 설치 로직
                    InstallMagicCanon();
                    magicCanonState = MagicCanonState.Active;
                    magicCanonTimer = 0f;
                    break;

                case MagicCanonState.Active:
                    magicCanonTimer += Time.deltaTime;
                    
                    // 10초 지속 시간 체크
                    if (magicCanonTimer >= magicCanonDuration)
                    {
                        magicCanonState = MagicCanonState.Finished;
                    }
                    break;

                case MagicCanonState.AOE:
                    // AOE 공격은 이미 ExecuteAOE()에서 처리됨
                    magicCanonState = MagicCanonState.Active; // 다시 Active 상태로 돌아감
                    break;

                case MagicCanonState.Finished:
                    magicCanonState = MagicCanonState.None;
                    AttackFactory.Instance.Deactivate(attack);
                    break;
            }
        }

        private void InstallMagicCanon()
        {
            // 마법 포탑을 해당 위치에 설치
            if (magicCanon != null)
            {
                magicCanon.transform.position = magicCanonPosition;
                magicCanon.SetActive(true);
            }
            
            Debug.Log($"[MagicCanon] 마법 포탑이 위치 {magicCanonPosition}에 설치되었습니다.");
        }

        private void ExecuteAOE()
        {
            // 마법 포탑 위치를 중심으로 magicCanonRadius 반경의 적들에게 AOE 공격을 발사합니다.
            var aoeAttack = AttackFactory.Instance.Create(AOEAttackData, attack.attacker, null, Vector2.zero);
            var aoeComponent = aoeAttack.components[0] as AC100_AOE;
            aoeComponent.aoeRadius = magicCanonRadius;
            aoeComponent.aoeDamage = (int)character.GetStatValue(StatType.AttackPower);
            aoeComponent.aoeDuration = 1f;
            aoeComponent.aoeInterval = 1f;
            aoeComponent.aoeTargetType = AOETargetType.AreaAtPosition;
            aoeComponent.aoeShapeType = AOEShapeType.Circle;
            aoeComponent.SetAOEPosition((Vector2)magicCanonPosition);
            aoeComponent.aoeVFXPrefab = magicCanonAOEParticle;
            aoeComponent.Activate(aoeAttack, Vector2.zero);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            magicCanon.SetActive(false);
            magicCanonState = MagicCanonState.None;
            magicCanonTimer = 0f;
            magicCanonPosition = Vector3.zero;
        }
    }
} 