using Stats;
using UnityEngine;

namespace CharacterSystem
{
    /// <summary>
    /// 사망 시 골드를 드랍하는 기본 적 캐릭터
    /// </summary>
    public class E005_BlueGolem : Enemy
    {
        private static readonly int AnimatorKey_Horizontal = Animator.StringToHash("Horizontal");

        [Header("Material Control")]
        [SerializeField] private Material material;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        // ===== [기능 2] 초기화 =====
        protected override void Start()
        {
            base.Start();
            
            // 머티리얼 초기화
            Animator.SetFloat(AnimatorKey_Horizontal,  -1);
            if (spriteRenderer == null)
                spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (material == null)
                material = spriteRenderer.material;
        }
        
        /// <summary>
        /// 글로우 효과 설정
        /// </summary>
        /// <param name="glowColor">글로우 컬러</param>
        /// <param name="glowIntensity">글로우 강도 (0-100)</param>
        public void SetGlowEffect(Color glowColor, float glowIntensity, float time = 1f)
        {
            // 해당 time 동안 글로우 효과 적용
            if (material != null)
            {
                material.EnableKeyword("GLOW_ON");
                material.SetColor("_GlowColor", glowColor);
                material.SetFloat("_Glow", Mathf.Clamp(glowIntensity, 0f, 100f));
                material.SetFloat("_GlowGlobal", 1f);
            }
        }

        private string GetStateString(string state)
        {
            switch (state)
            {
                case "IDLE":
                    return "IDLE";
                case "MOVE":
                    return "Movement";
                case "ATTACK":
                    return "Attack";
                case "DAMAGE":
                    return "DAMAGE";
                case "DEATH":
                    return "DEATH";
                default:
                    return "IDLE";
            }
        }
        protected override void ChangeAnimationState(string newState)
        {          
            if (Animator != null && Animator.HasState(0, Animator.StringToHash(GetStateString(newState))))
            {
                Animator.speed = 1f;
                // switch로 각 newStat에 대한 Parameter 값을 변경
                switch (newState)
                {
                    case "IDLE":
                        break;
                    case "MOVE":
                        if(Animator.GetBool("Attack")) return;
                        Animator.SetTrigger("Move");
                        break;
                    case "ATTACK":
                        float attackSpeed = GetStatValue(StatType.AttackSpeed);
                        Animator.speed = Mathf.Max(0f, attackSpeed / 10f);
                        Animator.SetBool("Move", false);
                        Animator.SetBool("Attack", true);
                        break;
                }
                currentAnimationState = newState;
            }
        }

        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override bool OnEvent(Utils.EventType eventType, object param)
        {
            var result = base.OnEvent(eventType, param);

            if (result)
            {
                if (eventType == Utils.EventType.OnDamaged)
                {
                    SetGlowEffect(Color.red, 10f);
                    Debug.Log("SetGlowEffect");
                }

                if (eventType == Utils.EventType.OnDeath)
                {
                    gameObject.SetActive(false);
                }
                return true;
            }
            return result;
        }
    }
}