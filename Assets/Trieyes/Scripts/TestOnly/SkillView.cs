using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using GamePlayer;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace OutGame
{
    public class SkillView : MonoBehaviour
    {
        [Header("Skill UI References")]
        public GameObject characterSkillTransform;
        public GameObject unlockSkillPrefab;
        public GameObject lockedSkillPrefab;
        public GameObject OverlayTextUIPrefab;

        private JsonToAchivement jsonToAchivement;
        private List<GameObject> skillViews = new List<GameObject>();

        private GameObject currentSkillView;
        private GameObject currentOverlayText;
        private float hideOverlayTime = 0f;

        private const string AddressablePath = "Assets/Trieyes/Addressable/Icons/Skills/";

        public void Awake()
        {
            jsonToAchivement = Player.Instance.jsonToAchivement;
        }

        private void Update()
        {
            if (hideOverlayTime > 0f && Time.time >= hideOverlayTime)
            {
                HideOverlay();
            }
        }

        private void HideOverlay()
        {
            if (currentOverlayText != null)
            {
                Destroy(currentOverlayText);
                currentOverlayText = null;
            }
            currentSkillView = null;
            hideOverlayTime = 0f;
        }

        public void UpdateSkillInfo(IAchievementObject characterAchievement)
        {
            if (characterSkillTransform == null || characterAchievement == null || jsonToAchivement == null) return;
            
            ClearSkillViews();
            if (!characterAchievement.IsUnlocked) return;

            // 캐릭터 스킬과 공용 스킬 표시
            var allSkills = jsonToAchivement.GetAchievementsByType(JsonToAchivement.SKILL_TYPE);
            foreach (var skill in allSkills)
            {
                if (skill.Dependency == characterAchievement.Id || skill.Dependency == -1)
                {
                    CreateSkillView(skill);
                }
            }
        }

        private void CreateSkillView(IAchievementObject skill)
        {
            if (unlockSkillPrefab == null || lockedSkillPrefab == null) return;

            GameObject prefabToUse = skill.IsUnlocked ? unlockSkillPrefab : lockedSkillPrefab;
            GameObject skillViewObj = Instantiate(prefabToUse, characterSkillTransform.transform);
            
            SetupSkillView(skillViewObj, skill);
            SetupMouseEvents(skillViewObj, skill);
            skillViews.Add(skillViewObj);
        }

        private void SetupSkillView(GameObject skillViewObj, IAchievementObject skill)
        {
            var textComponent = skillViewObj.GetComponentInChildren<TextMeshProUGUI>();
            var image = skillViewObj.GetComponentInChildren<Image>();
            
            if (textComponent != null)
            {
                textComponent.text = skill.IsUnlocked ? skill.Name : "???";
            }

            if (image != null && skill is SkillAchievement skillAchievement)
            {
                if (!string.IsNullOrEmpty(skillAchievement.skillIconAddressable))
                {
                    LoadSkillIcon(image, skillAchievement.skillIconAddressable, !skill.IsUnlocked);
                }
                else
                {
                    image.color = skill.IsUnlocked ? Color.white : Color.gray;
                }
            }
        }

        private void LoadSkillIcon(Image image, string addressableKey, bool isLocked = false)
        {
            string addressablePath = AddressablePath + addressableKey + ".png";
                    
            // Addressable에서 스프라이트 로드 시도
            var handle = Addressables.LoadAssetAsync<Sprite>(addressablePath);
            handle.Completed += (AsyncOperationHandle<Sprite> spriteHandle) =>
            {
                if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    image.sprite = spriteHandle.Result;
                    Debug.Log($"스프라이트 로드 성공: {addressablePath}");
                }
                else
                {
                    Debug.LogWarning($"스프라이트 로드 실패: {addressablePath}");
                }
            };

            image.color = isLocked ? Color.gray : Color.white;
        }

        private void SetupMouseEvents(GameObject skillViewObj, IAchievementObject skill)
        {
            var eventTrigger = skillViewObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = skillViewObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => { OnSkillMouseEnter(skill); });
            eventTrigger.triggers.Add(pointerEnter);

            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => { OnSkillMouseExit(); });
            eventTrigger.triggers.Add(pointerExit);
        }

        private void OnSkillMouseEnter(IAchievementObject skill)
        {
            if (currentOverlayText != null) return;

            // 현재 스킬 뷰 설정
            currentSkillView = GetCurrentSkillView();
            if (currentSkillView == null) return;

            // 오버레이 생성
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;
            
            currentOverlayText = Instantiate(OverlayTextUIPrefab, canvas.transform);
            
            var textComponent = currentOverlayText.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = skill.IsUnlocked ? skill.Description : skill.UnlockConditionDescription;
            }

            // 오버레이 위치 설정 및 이벤트 설정
            SetOverlayPosition();
            SetupOverlayEvents();
        }

        private void OnSkillMouseExit()
        {
            // 0.2초 후 오버레이 삭제 예약
            hideOverlayTime = Time.time + 0.2f;
        }

        private GameObject GetCurrentSkillView()
        {
            foreach (var skillView in skillViews)
            {
                if (skillView != null && skillView.activeInHierarchy)
                {
                    var rectTransform = skillView.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Vector2 localPoint;
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            rectTransform, Input.mousePosition, null, out localPoint);
                        if (rectTransform.rect.Contains(localPoint)) return skillView;
                    }
                }
            }
            return null;
        }

        private void SetOverlayPosition()
        {
            if (currentOverlayText == null || currentSkillView == null) return;

            var skillNameText = currentSkillView.GetComponentInChildren<TextMeshProUGUI>();
            if (skillNameText == null) return;

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, skillNameText.transform.position),
                canvas.worldCamera, out localPoint);

            currentOverlayText.GetComponent<RectTransform>().anchoredPosition = new Vector2(localPoint.x, localPoint.y - 50);
        }

        private void SetupOverlayEvents()
        {
            if (currentOverlayText == null) return;

            var eventTrigger = currentOverlayText.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null) eventTrigger = currentOverlayText.AddComponent<UnityEngine.EventSystems.EventTrigger>();

            var enter = new UnityEngine.EventSystems.EventTrigger.Entry();
            enter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            enter.callback.AddListener((data) => { hideOverlayTime = 0f; });
            eventTrigger.triggers.Add(enter);

            var exit = new UnityEngine.EventSystems.EventTrigger.Entry();
            exit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            exit.callback.AddListener((data) => { HideOverlay(); });
            eventTrigger.triggers.Add(exit);
        }

        private void ClearSkillViews()
        {
            HideOverlay();
            foreach (var skillView in skillViews)
            {
                if (skillView != null) Destroy(skillView);
            }
            skillViews.Clear();
        }

        public void HideSkillInfo()
        {
            ClearSkillViews();
        }
    }
}
