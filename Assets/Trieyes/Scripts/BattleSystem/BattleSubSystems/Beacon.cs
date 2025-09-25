using UnityEngine;
using CharacterSystem;
using System;

namespace BattleSystem
{
    /// <summary>
    /// 비콘 오브젝트 - 캐릭터가 일정 시간 동안 머물면 BattleTimerStage에 콜백을 보내는 컴포넌트
    /// </summary>
    public class Beacon : MonoBehaviour
    {
        [Header("Beacon Settings")]
        [SerializeField] private float durationCharacterStay = 3f;        // 캐릭터가 머물어야 하는 시간
        [SerializeField] private float durationCharacterReEnterTolerance = 1f;       // 캐릭터가 나간 후 다시 들어올 수 있는 시간
        
        [Header("Debug")]
        [SerializeField] private bool isCharacterInside = false;
        [SerializeField] private float currentTimer = 0f;
        [SerializeField] private float lastExitTime = 0f;
        
        // 콜백 이벤트
        public Action<Beacon> OnBeaconActivated;
        
        // 내부 상태
        private Character currentCharacter;
        private bool isActivated = false;
        private CircleCollider2D circleCollider;

        public bool IsActivated => isActivated;
        public float Progress => isActivated ? 1f : (currentTimer / durationCharacterStay);
        public float RemainingTime => isActivated ? 0f : Mathf.Max(0f, durationCharacterStay - currentTimer);
    
        private Color startColor;
        private Color triggerColor = Color.red;

        private void Awake()
        {
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleCollider == null)
            {
                Debug.LogError("Beacon requires CircleCollider2D component!");
            }
            else
            {
                circleCollider.isTrigger = true;
            }

             var particleSystem = GetComponent<ParticleSystem>();
             if (particleSystem != null)
             {
                 startColor = particleSystem.main.startColor.color;
             }
        }
        
        private void Update()
        {
            if (isActivated)
                return;
                
            if (isCharacterInside && currentCharacter != null)
            {
                // 캐릭터가 비콘 안에 있는 동안 타이머 증가
                currentTimer += Time.deltaTime;
                
                // duration 시간이 지나면 비콘 활성화
                if (currentTimer >= durationCharacterStay)
                {
                    ActivateBeacon();
                }
            }
            else if (!isCharacterInside && currentCharacter == null)
            {
                // 캐릭터가 나간 상태에서 durationCharacterReEnterTolerance 시간이 지나면 타이머 초기화
                if (Time.time - lastExitTime >= durationCharacterReEnterTolerance)
                {
                    ResetTimer();
                }
            }
        }
        
         private void OnTriggerEnter2D(Collider2D other)
         {
             Character character = other.GetComponent<Character>();
             if (character == null)
                 return;
                 
             // 이미 다른 캐릭터가 있거나 비콘이 활성화된 경우 무시
             if (currentCharacter != null || isActivated)
                 return;
                 
             currentCharacter = character;
             isCharacterInside = true;

             var particleSystem = GetComponent<ParticleSystem>();
             if (particleSystem != null)
             {
                 var main = particleSystem.main;
                 main.startColor = triggerColor;
             }

             // 캐릭터가 나간 후 durationCharacterReEnterTolerance 시간 내에 다시 들어온 경우
             if (Time.time - lastExitTime <= durationCharacterReEnterTolerance && lastExitTime > 0f)
             {
                 // 기존 타이머를 그대로 유지
                 Debug.Log($"Character re-entered within durationCharacterReEnterTolerance. Timer continues: {currentTimer:F2}s");
             }
             else
             {
                 // durationCharacterReEnterTolerance 시간이 지났으면 타이머 초기화
                 ResetTimer();
                 Debug.Log("Character entered after durationCharacterReEnterTolerance. Timer reset.");
             }
             
             Debug.Log($"Character entered beacon. Current timer: {currentTimer:F2}s / {durationCharacterStay:F2}s");
         }
        
         private void OnTriggerExit2D(Collider2D other)
         {
             Character character = other.GetComponent<Character>();
             if (character == null || character != currentCharacter)
                 return;
             
             var particleSystem = GetComponent<ParticleSystem>();
             if (particleSystem != null)
             {
                 var main = particleSystem.main;
                 main.startColor = startColor;
             }
                 
             currentCharacter = null;
             isCharacterInside = false;
             lastExitTime = Time.time;
             
             Debug.Log($"Character exited beacon. Timer paused at: {currentTimer:F2}s / {durationCharacterStay:F2}s");
         }
        
        private void ActivateBeacon()
        {
            if (isActivated)
                return;
                
            isActivated = true;
            Debug.Log($"Beacon activated! Duration: {currentTimer:F2}s");
            
            // BattleTimerStage에 콜백 전송
            OnBeaconActivated?.Invoke(this);
            
            // 비콘 파괴
            Destroy(gameObject);
        }
        
        private void ResetTimer()
        {
            currentTimer = 0f;
        }
        
        public void Initialize(float durationCharacterStay = 10f, float durationCharacterReEnterTolerance = 3f)
        {
            this.durationCharacterStay = durationCharacterStay;
            this.durationCharacterReEnterTolerance = durationCharacterReEnterTolerance;
            ResetTimer();
        }
    }
}
