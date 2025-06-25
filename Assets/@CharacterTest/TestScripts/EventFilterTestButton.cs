using UnityEngine;
using UnityEngine.UI;
using CharacterSystem;
using Utils;
using System.Collections.Generic;
using System.Linq;

namespace CharacterTest
{
    /// <summary>
    /// 이벤트 필터링 시스템을 테스트하기 위한 버튼 컴포넌트입니다.
    /// Inspector에서 이벤트 타입 리스트와 Character 프리팹을 설정하여 테스트할 수 있습니다.
    /// </summary>
    public class EventFilterTestButton : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private List<Utils.EventType> testEvents = new List<Utils.EventType>();
        [SerializeField] private Pawn targetCharacter;
        
        [Header("Test Results")]
        [SerializeField] private bool showDetailedLogs = true;
        
        private Button button;
        private Text buttonText;
        
        private void Start()
        {
            // 컴포넌트 찾기
            button = GetComponent<Button>();
            buttonText = GetComponentInChildren<Text>();
            
            // 버튼 클릭 이벤트 등록
            if (button != null)
            {
                button.onClick.AddListener(OnTestButtonClicked);
                UpdateButtonText();
            }
            else
            {
                Debug.LogError("<color=red>[EVENT_FILTER_TEST] Button component not found!</color>");
            }
        }
        
        private void UpdateButtonText()
        {
            if (buttonText != null)
            {
                string characterName = targetCharacter != null ? targetCharacter.gameObject.name : "None";
                string eventCount = testEvents.Count.ToString();
                buttonText.text = $"Test Events ({eventCount})\nTarget: {characterName}";
            }
        }
        
        /// <summary>
        /// 테스트 버튼 클릭 시 호출되는 메서드입니다.
        /// 설정된 이벤트들을 targetCharacter에 발생시킵니다.
        /// </summary>
        public void OnTestButtonClicked()
        {
            if (targetCharacter == null)
            {
                Debug.LogError("<color=red>[EVENT_FILTER_TEST] Target character is not set!</color>");
                return;
            }
            
            if (testEvents.Count == 0)
            {
                Debug.LogWarning("<color=yellow>[EVENT_FILTER_TEST] No test events configured!</color>");
                return;
            }
            
            Debug.Log($"<color=green>[EVENT_FILTER_TEST] Starting event test for {targetCharacter.gameObject.name}</color>");
            Debug.Log($"<color=green>[EVENT_FILTER_TEST] Target character accepted events: {string.Join(", ", targetCharacter.GetAcceptedEvents())}</color>");
            Debug.Log($"<color=green>[EVENT_FILTER_TEST] Target character card events: {string.Join(", ", targetCharacter.GetCardAcceptedEvents())}</color>");
            Debug.Log($"<color=green>[EVENT_FILTER_TEST] Target character relic events: {string.Join(", ", targetCharacter.GetRelicAcceptedEvents())}</color>");
            
            // 각 테스트 이벤트를 순차적으로 발생시킴
            foreach (var eventType in testEvents)
            {
                TriggerTestEvent(eventType);
            }
            
            Debug.Log($"<color=green>[EVENT_FILTER_TEST] Event test completed!</color>");
        }
        
        /// <summary>
        /// 특정 이벤트를 targetCharacter에 발생시킵니다.
        /// </summary>
        /// <param name="eventType">발생시킬 이벤트 타입</param>
        private void TriggerTestEvent(Utils.EventType eventType)
        {
            Debug.Log($"<color=blue>[EVENT_FILTER_TEST] Triggering event: {eventType}</color>");
            
            // 이벤트 발생 (param은 null)
            targetCharacter.OnEvent(eventType, null);
            
            // 상세 로그 출력
            if (showDetailedLogs)
            {
                LogEventDetails(eventType);
            }
        }
        
        /// <summary>
        /// 이벤트 상세 정보를 로그로 출력합니다.
        /// </summary>
        /// <param name="eventType">이벤트 타입</param>
        private void LogEventDetails(Utils.EventType eventType)
        {
            Debug.Log($"<color=cyan>[EVENT_FILTER_TEST] Event Details - Type: {eventType}</color>");
            
            // 이벤트 필터링 상태 확인
            bool isAccepted = targetCharacter.GetAcceptedEvents().Contains(eventType);
            bool isCardAccepted = targetCharacter.GetCardAcceptedEvents().Contains(eventType);
            bool isRelicAccepted = targetCharacter.GetRelicAcceptedEvents().Contains(eventType);
            
            Debug.Log($"<color=cyan>[EVENT_FILTER_TEST] Filter Status - Accepted: {isAccepted}, Card: {isCardAccepted}, Relic: {isRelicAccepted}</color>");
        }
        
        /// <summary>
        /// Inspector에서 설정이 변경될 때 호출됩니다.
        /// </summary>
        private void OnValidate()
        {
            UpdateButtonText();
        }
        
        /// <summary>
        /// 특정 이벤트를 개별적으로 테스트할 수 있는 public 메서드입니다.
        /// </summary>
        /// <param name="eventType">테스트할 이벤트 타입</param>
        public void TestSingleEvent(Utils.EventType eventType)
        {
            if (targetCharacter != null)
            {
                TriggerTestEvent(eventType);
            }
        }
        
        /// <summary>
        /// 모든 이벤트 타입을 테스트할 수 있는 public 메서드입니다.
        /// </summary>
        public void TestAllEvents()
        {
            if (targetCharacter != null)
            {
                foreach (Utils.EventType eventType in System.Enum.GetValues(typeof(Utils.EventType)))
                {
                    TriggerTestEvent(eventType);
                }
            }
        }
    }
} 