using UnityEngine;
using UnityEngine.UI;
using CharacterSystem;
using CardViews;
using GameFramework;

namespace NodeStage
{
    public abstract class EventStage : MonoBehaviour, NodeStage
    {
        [Header("공통 UI")]
        [SerializeField] protected RectTransform rectTransform;
        [SerializeField] protected Button openDeckButton;   // 좌상단 덱 버튼(옵션)
        [SerializeField] protected DeckView deckView;       // 공유 DeckView(옵션)

        [Header("다음 스테이지 타입")]
        [SerializeField] protected StageType stageType;     // 인스펙터에서 스테이지마다 지정

        [HideInInspector]public Character mainCharacter;

        protected virtual void Awake()
        {
            if (rectTransform != null) rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
        }

        public virtual void Activate(Character mainCharacter)
        {
            this.mainCharacter = mainCharacter;

            // 덱 보기 버튼 바인딩(중복 방지)
            if (openDeckButton != null)
            {
                openDeckButton.onClick.RemoveAllListeners();
                openDeckButton.onClick.AddListener(OpenDeckInspectOnly);
            }

            OnActivated();
            gameObject.SetActive(true);
        }

        // 파생에서 초기화/세팅
        protected virtual void OnActivated() { }

        protected virtual void Deactivate()
        {
            OnDeactivated();
            gameObject.SetActive(false);
        }

        // 파생에서 정리/해제
        protected virtual void OnDeactivated() { }

        public virtual void NextStage()
        {
            Deactivate();
            NextStageSelectPopup.Instance.SetNextStage(stageType, mainCharacter);
        }

        // 좌상단 덱 버튼 → 읽기 전용 덱 뷰
        protected void OpenDeckInspectOnly()
        {
            if (deckView == null || mainCharacter == null) return;
            deckView.Activate(mainCharacter.deck, requiredCount: 0, onConfirm: null, onCancel: null);
        }
        protected virtual void OnDestroy() { }
    }
    
    public abstract class EventStage<T> : EventStage where T : EventStage<T>
    {
        public static T Instance { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != null && Instance != (T)this)
            {
                // 한 씬에 두 개 이상 생기면 이전 패턴과 동일하게 처리
                Destroy(gameObject); 
                return;
            }
            Instance = (T)this;
        }

        protected override void OnDestroy()
        {
            if (Instance == (T)this) Instance = null;
            base.OnDestroy();
        }
    }
}