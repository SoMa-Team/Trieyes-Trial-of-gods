using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using OutGame;
using UnityEngine.EventSystems;

namespace AttackSystem
{
    /// <summary>
    /// 스킬 UI 정보 표시, 선택/클릭/강조 등 스킬 단위 뷰 관리 클래스
    /// </summary>
    public class SkillView : MonoBehaviour, IPointerClickHandler
    {
        // =================== [UI 참조] ===================
        private bool canInteract;
        
        [Header("스킬 전체")]
        public RectTransform rectTransform;
        
        [Header("스킬 기본 UI")]
        public Image skillIcon;
        public TMP_Text titleText;
        public TMP_Text descriptionText;
        public Image backgroundImage;
        public Image borderImage;
        public Image selectionOutline;
        
        [Header("시각적 설정")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.yellow;
        [SerializeField] private Color disabledColor = Color.gray;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f, 1f);

        // =================== [내부 상태] ===================
        private IAchievementObject skillData;
        private AttackData attackData;
        private bool isSelected = false;
        private bool isHovered = false;
        private Action<SkillView> onSkillClicked;

        public IAchievementObject SkillData => skillData;
        public AttackData AttackData => attackData;
        public bool IsSelected => isSelected;
        public bool IsInteractable => canInteract;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
        }

        // =============== [스킬 기본/상태 관리] ===============
        #region SkillView 로직

        /// <summary>스킬 설정 및 UI 갱신</summary>
        public virtual void SetSkillData(IAchievementObject skill)
        {
            canInteract = false;
            this.skillData = skill;
            SetSelected(false);
            UpdateView();
        }

        /// <summary>클릭 이벤트 콜백 설정</summary>
        public void SetOnClicked(Action<SkillView> handler)
        {
            onSkillClicked = handler;
        }
        
        /// <summary>AttackData 정보 설정</summary>
        public void SetAttackDataInfo(AttackData attackData)
        {
            this.attackData = attackData;
            UpdateView(); // AttackData 정보가 변경되면 UI 업데이트
        }

        /// <summary>상호작용 가능 여부 설정</summary>
        public void SetCanInteract(bool canInteract)
        {
            this.canInteract = canInteract;
        }

        /// <summary>현재 스킬 데이터 반환</summary>
        public IAchievementObject GetCurrentSkill()
        {
            if (skillData == null) Debug.LogError("SkillView.GetCurrentSkill: skillData is null");
            return skillData;
        }

        /// <summary>스킬 정보 및 UI 전반 갱신</summary>
        public void UpdateView()
        {
            if (skillData == null) return;

            // 스킬 이름 설정
            if (titleText != null)
            {
                titleText.text = skillData.IsUnlocked ? skillData.Name : "???";
            }

            // 스킬 설명 설정
            if (descriptionText != null)
            {
                descriptionText.text = skillData.IsUnlocked ? skillData.Description : skillData.UnlockConditionDescription;
            }

            // 스킬 아이콘 로드
            if (skillData is SkillAchievement skillAchievementIcon)
            {
                if (!string.IsNullOrEmpty(skillAchievementIcon.skillIconAddressable))
                {
                    LoadSkillIcon(skillAchievementIcon.skillIconAddressable);
                }
            }

            // 시각적 상태 업데이트
            UpdateVisualState();
        }

        /// <summary>선택 상태 설정 및 시각적 강조</summary>
        public void SetSelected(bool selected)
        {
            if (!canInteract)
                return;
            
            isSelected = selected;
            UpdateVisualState();
        }

        #endregion

        // =============== [스킬 아이콘 관리] ===============
        #region 아이콘 로딩

        /// <summary>스킬 아이콘을 설정합니다.</summary>
        public void SetSkillIcon(Sprite sprite)
        {
            if (skillIcon != null)
            {
                skillIcon.sprite = sprite;
            }
        }

        /// <summary>스킬 아이콘을 Addressable로 로드합니다.</summary>
        public void LoadSkillIcon(string iconKey)
        {
            if (string.IsNullOrEmpty(iconKey) || skillIcon == null) return;

            // Addressable에서 스킬 아이콘 로드
            string addressablePath = $"Assets/Trieyes/Addressable/Icons/Skills/{iconKey}.png";
            
            UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<Sprite>(addressablePath).Completed += (handle) =>
            {
                if (handle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    skillIcon.sprite = handle.Result;
                }
                else
                {
                    Debug.LogWarning($"스킬 아이콘 로드 실패: {addressablePath}");
                }
            };
        }

        #endregion

        // =============== [시각적 상태 관리] ===============
        #region 시각적 효과

        private void UpdateVisualState()
        {
            Color targetColor = normalColor;

            if (!canInteract)
            {
                targetColor = disabledColor;
            }
            else if (isSelected)
            {
                targetColor = selectedColor;
            }
            else if (isHovered)
            {
                targetColor = hoverColor;
            }

            // 배경 색상 업데이트
            if (backgroundImage != null)
            {
                backgroundImage.color = targetColor;
            }

            // 테두리 색상 업데이트
            if (borderImage != null)
            {
                borderImage.color = targetColor;
            }

            // 선택 테두리 표시
            if (selectionOutline != null)
            {
                selectionOutline.color = isSelected ? selectedColor : new Color(246f/255f, 220f/255f, 168f/255f, 1f);
            }

            // 잠금 상태에 따른 아이콘 색상
            if (skillIcon != null && skillData != null)
            {
                skillIcon.color = skillData.IsUnlocked ? Color.white : Color.gray;
            }
        }

        #endregion

        // =============== [이벤트 처리] ===============
        #region 포인터 이벤트

        /// <summary>포인터 클릭 이벤트 처리</summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!canInteract) return;

            onSkillClicked?.Invoke(this);
        }

        /// <summary>포인터 진입 이벤트 처리</summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!canInteract) return;

            isHovered = true;
            UpdateVisualState();
        }

        /// <summary>포인터 나가기 이벤트 처리</summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (!canInteract) return;

            isHovered = false;
            UpdateVisualState();
        }

        #endregion

        // =============== [유틸리티 메서드] ===============
        #region 유틸리티

        /// <summary>스킬 뷰를 초기화합니다.</summary>
        public void Initialize()
        {
            isSelected = false;
            isHovered = false;
            canInteract = true;
            skillData = null;
            attackData = null;
            
            if (titleText != null) titleText.text = "";
            if (descriptionText != null) descriptionText.text = "";
            if (skillIcon != null) skillIcon.sprite = null;
            
            UpdateVisualState();
        }

        /// <summary>현재 스킬의 ID를 반환합니다.</summary>
        public int GetSkillId()
        {
            return skillData?.Id ?? -1;
        }

        #endregion
    }
}