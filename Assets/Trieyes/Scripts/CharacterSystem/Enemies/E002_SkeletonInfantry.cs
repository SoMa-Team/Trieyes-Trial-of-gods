using AttackSystem;
using UnityEngine;
using Utils;
using System.Linq;
using BattleSystem;
using Stats;

namespace CharacterSystem
{
    /// <summary>
    /// 사망 시 골드를 드랍하는 기본 적 캐릭터
    /// </summary>
    public class E002_SkeletonInfantry : Enemy
    {        
        [Header("Material Control")]
        [SerializeField] private Material material;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        // ===== [기능 2] 초기화 =====
        protected override void Start()
        {
            base.Start();
            
            // 머티리얼 초기화
            if (spriteRenderer == null)
                spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
            if (material == null)
                material = spriteRenderer.material;
        }

        // ===== [커스텀 메서드] =====
        /// <summary>
        /// 오브젝트 풀링을 위한 활성화 함수
        /// </summary>
        public override void Activate()
        {
            base.Activate();
            dropGold = 10;
            // TODO: AttackComponent 할당
            ////Debug.Log("Enemy001 Activated.");

            // 이런 느낌으로 각 적마다 커스터마이징 
            // boxCollider = Collider as BoxCollider2D;
        }

        /// <summary>
        /// 오브젝트 풀링을 위한 비활성화 함수
        /// </summary>
        public override void Deactivate()
        {
            // Enemy001 고유 정리 로직
            dropGold = 10; // 기본값으로 초기화
            
            base.Deactivate();
            ////Debug.Log("Enemy001 Deactivated.");
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

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
        }

        /// <summary>
        /// 이벤트를 처리합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        /// <param name="param">이벤트 파라미터</param>
        public override void OnEvent(Utils.EventType eventType, object param)
        {
            base.OnEvent(eventType, param);

            if (eventType == Utils.EventType.OnDamaged)
            {
                SetGlowEffect(Color.red, 10f);
                Debug.Log("SetGlowEffect");
            }

            if (eventType == Utils.EventType.OnDeath)
            {
                EnemyFactory.Instance.Deactivate(this);
            }
        }

        // ===== [이벤트 처리 메서드] =====
        /// <summary>
        /// 사망 시 골드 드랍 처리
        /// </summary>
        /// <param name="param">이벤트 파라미터</param>
        protected override void OnSelfDeath(AttackResult result)
        {
            base.OnSelfDeath(result);
            
        }
    }
} 
