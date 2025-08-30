using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using StickerSystem;
using Utils;

namespace CardViews
{
    /// <summary>
    /// 카드 UI 정보 표시, 선택/클릭/강조/스티커 적용 등 카드 단위 뷰 관리 클래스
    /// </summary>
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        // =================== [UI 참조] ===================
        private bool canInteract;
        
        [Header("카드 전체")]
        public RectTransform rectTransform;
        
        [Header("카드 기본 UI")]
        public Image illustrationImage;
        public TMP_Text cardNameText;
        public TMP_Text descriptionText;

        [Header("스탯/레어리티/특성 UI")]
        public StatTypeEmblemSO statTypeEmblemTable;

        public GameObject StatTypePair1;
        public GameObject StatTypePair2;
        public Image statTypeEmblemImage1;
        public TMP_Text statIntegerValueText1;
        public Image statTypeEmblemImage2;
        public TMP_Text statIntegerValueText2;
        public Image selectionOutline;

        [Header("오버레이/엠블럼 프리팹")]
        public List<GameObject> stickerOverlayPrefabs;    // [0]=None, [1]=StatType, [2]=Number
        public List<GameObject> propertyTypeEmblems;      // [0]=Fire, [1]=Ice, [2]=Light, [3]=Dark, [4]=Steel
        public List<GameObject> rarityEmblems;            // 카드 레어리티 별 엠블럼

        // =================== [내부 상태] ===================
        private Card card;
        private readonly List<GameObject> activeStickerOverlays = new();
        
        private int appliedParamIdxForPickMode = -1;
        private readonly Dictionary<int, StickerSystem.Sticker> previewReplacedBackup = new();

        // =================== [상수/옵션] ===================
        private static readonly Vector2 OVERLAY_PADDING      = new(15f, 15f);
        private static readonly Vector2 STICKER_PADDING      = new(0f, 0f);
        private const float OVERLAY_X_OFFSET                 = -5f;
        private const float OVERLAY_Y_OFFSET                 = 0f;
        private const float BG_X_OFFSET                      = 0f;
        private const float BG_Y_OFFSET                      = 0f;
        private const float STICKER_OVERLAY_FIXED_HEIGHT     = 58.28125f;
        private const float NUMBER_STICKER_OVERLAY_Y_OFFSET  = -5.1719f;
        
        private bool paramPickMode = false;
        private Action<int> onParamPicked;     // 선택된 파라미터 idx 콜백
        private StickerSystem.Sticker pickModeSticker;

        public void EnableParamPickMode(StickerSystem.Sticker sticker, Action<int> onPicked)
        {
            paramPickMode = true;
            pickModeSticker = sticker;
            onParamPicked = onPicked;
            appliedParamIdxForPickMode = -1;
            previewReplacedBackup.Clear();
        }

        public void DisableParamPickMode()
        {
            paramPickMode = false;
            pickModeSticker = null;
            onParamPicked = null;
            appliedParamIdxForPickMode = -1;
            previewReplacedBackup.Clear();
        }

        // =============== [오버레이 생성/관리] ===============
        #region Overlay 생성 및 관리

        /// <summary>
        /// 파라미터(스티커 등)에 맞는 오버레이 UI 생성
        /// </summary>
        private void CreateParamOverlays(
            Dictionary<int, List<int>> groupCharsByLineNum, TMP_TextInfo textInfo,
            StickerType stickerType, string paramText = null)
        {
            foreach (var lineKv in groupCharsByLineNum)
            {
                var charIndices = lineKv.Value;
                var firstCharInfo = textInfo.characterInfo[charIndices[0]];
                var lastCharInfo = textInfo.characterInfo[charIndices[^1]];

                Vector3 bl = firstCharInfo.bottomLeft;
                Vector3 tr = lastCharInfo.topRight;
                float width = tr.x - bl.x;
                float height = tr.y - bl.y;

                Vector2 padding = stickerType == StickerType.None ? OVERLAY_PADDING : OVERLAY_PADDING + STICKER_PADDING;
                Vector2 overlayOffset = stickerType == StickerType.None ? new(BG_X_OFFSET, BG_Y_OFFSET) : new(OVERLAY_X_OFFSET, OVERLAY_Y_OFFSET);

                Vector2 overlaySize = new(
                    width + padding.x,
                    (stickerType == StickerType.None) ? Mathf.Abs(height) + padding.y : STICKER_OVERLAY_FIXED_HEIGHT
                );
                Vector2 overlayPos = bl + new Vector3(-padding.x * 0.5f, -padding.y * 0.5f, 0);
                if (stickerType == StickerType.Number)
                    overlayPos.y += NUMBER_STICKER_OVERLAY_Y_OFFSET;
                overlayPos += overlayOffset;

                // === 오버레이 프리팹 Instantiate ===
                int prefabIndex = (int)stickerType;
                var prefab = stickerOverlayPrefabs[prefabIndex];
                var go = Instantiate(prefab);

                var descRT = descriptionText.rectTransform;
                var rt = go.GetComponent<RectTransform>();
                rt.sizeDelta = overlaySize;
                rt.localScale = Vector3.one;

                if (stickerType == StickerType.None)
                {
                    var parentRT = descRT.parent as RectTransform;
                    go.transform.SetParent(parentRT, false);
                    go.transform.SetAsFirstSibling();
                    rt.anchoredPosition = overlayPos + descRT.anchoredPosition;
                }
                else
                {
                    go.transform.SetParent(descRT, false);
                    go.transform.SetAsLastSibling();
                    rt.localScale = Vector3.one * 1.1f;
                    rt.anchoredPosition = overlayPos;
                }
                rt.anchorMin = descRT.anchorMin;
                rt.anchorMax = descRT.anchorMax;
                rt.pivot = descRT.pivot;

                var stickerView = go.GetComponent<StickerOverlayView>();
                if (stickerView != null)
                    stickerView.UpdateText(paramText ?? "", descriptionText);

                activeStickerOverlays.Add(go);
            }
        }

        /// <summary>
        /// 모든 오버레이(스티커, BG 등) 싱크 및 갱신
        /// </summary>
        private void SyncStickerOverlays()
        {
            // 1. 기존 오버레이 삭제
            foreach (var go in activeStickerOverlays)
                Destroy(go);
            activeStickerOverlays.Clear();

            // 2. 파라미터별 오버레이 생성
            descriptionText.ForceMeshUpdate();
            var textInfo = descriptionText.textInfo;

            for (int paramIdx = 0; paramIdx < card.paramCharRanges.Count; paramIdx++)
            {
                var range = card.paramCharRanges[paramIdx];
                StickerType stickerType = StickerType.None;
                Sticker sticker;
                if (card.stickerOverrides != null && card.stickerOverrides.TryGetValue(paramIdx, out sticker))
                    stickerType = sticker.type;

                var groupCharsByLineNum = GroupCharsByLineNum(range.start, range.end, textInfo);
                string paramText = (stickerType != StickerType.None) ? card.GetEffectiveParamTexts()[paramIdx] : null;
                CreateParamOverlays(groupCharsByLineNum, textInfo, stickerType, paramText);
            }
        }
        #endregion

        // =============== [카드 기본/상태 관리] ===============
        #region CardView 로직

        /// <summary>카드 설정 및 UI 갱신</summary>
        public virtual void SetCard(Card card)
        {
            canInteract = false;
            this.card = card;
            SetSelected(false);
            UpdateView();
        }

        public void SetCanInteract(bool canInteract)
        {
            this.canInteract = canInteract;
        }

        /// <summary>현재 카드 반환</summary>
        public Card GetCurrentCard()
        {
            if (card == null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return card;
        }

        /// <summary>카드 정보 및 스티커, 오버레이, UI 전반 갱신</summary>
        public void UpdateView()
        {
            // 카드 일러스트, 이름, 강화레벨
            illustrationImage.sprite = card.illustration;
            cardNameText.text = card.cardName;
            if (card.cardEnhancement.level.Value > 1)
            {
                int plusLevel = card.cardEnhancement.level.Value - 1;
                cardNameText.text += $" <color=#FFD600>+{plusLevel}</color>";
            }

            // 카드 설명/파라미터
            var descParams = card.GetEffectiveParamTexts();
            descriptionText.text = FormatDescription(card.cardDescription, descParams);

            // 특성(속성) 엠블럼
            for (int i = 0; i < propertyTypeEmblems.Count; i++)
            {
                bool hasProperty = false;
                foreach (var property in card.properties)
                {
                    if (i == (int)property)
                    {
                        hasProperty = true;
                        break;
                    }
                }
                propertyTypeEmblems[i].gameObject.SetActive(hasProperty);
            }

            // 레어리티 엠블럼
            for (int i = 0; i < rarityEmblems.Count; i++)
                rarityEmblems[i].gameObject.SetActive(i == (int)card.rarity);

            // 스탯 엠블럼 및 값
            if (card.cardStats.stats.Count > 0 && statTypeEmblemTable != null)
            {
                var stat = card.cardStats.stats[0];
                statTypeEmblemImage1.sprite = statTypeEmblemTable.GetEmblem(stat.type);
                statTypeEmblemImage1.enabled = statTypeEmblemImage1.sprite != null;
                statIntegerValueText1.text = $"+{stat.value.Value}";
                statIntegerValueText1.enabled = true;
            }
            else
            {
                StatTypePair1.SetActive(false);
            }

            if (card.cardStats.stats.Count > 1 && statTypeEmblemTable != null)
            {
                var stat = card.cardStats.stats[1];
                statTypeEmblemImage2.sprite = statTypeEmblemTable.GetEmblem(stat.type);
                statTypeEmblemImage2.enabled = statTypeEmblemImage2.sprite != null;
                statIntegerValueText2.text = $"+{stat.value.Value}";
                statIntegerValueText2.enabled = true;
            }
            else
            {
                StatTypePair2.SetActive(false);
            }

            // 오버레이 싱크
            SyncStickerOverlays();
        }

        /// <summary>
        /// 텍스트(설명)의 지정된 범위(파라미터) 라인별로 그룹핑
        /// </summary>
        private Dictionary<int, List<int>> GroupCharsByLineNum(int start, int end, TMP_TextInfo textInfo)
        {
            Dictionary<int, List<int>> res = new();
            for (int i = start; i <= end; i++)
            {
                if (i < 0 || i >= textInfo.characterCount) continue;
                int lineNum = textInfo.characterInfo[i].lineNumber;
                if (!res.ContainsKey(lineNum))
                    res[lineNum] = new List<int>();
                res[lineNum].Add(i);
            }
            return res;
        }

        /// <summary>
        /// 파라미터 적용된 카드 설명 텍스트 생성
        /// </summary>
        private string FormatDescription(string template, List<string> descParams)
        {
            if (descParams == null || descParams.Count == 0)
                return template;

            var sb = new System.Text.StringBuilder(template);

            for (int i = descParams.Count - 1; i >= 0; i--)
                sb.Replace("{" + i + "}", descParams[i]);

            return sb.ToString();
        }

        /// <summary>
        /// 카드 설명 클릭 이벤트 (스티커 적용 및 카드 선택)
        /// </summary>
        public void OnPointerClick(PointerEventData eventData) // Todo: 깔끔하게 바꾸기
        {
            if (!canInteract) return;

            // 설명 영역 안을 눌렀는지 먼저 판정
            bool inDescription = RectTransformUtility.RectangleContainsScreenPoint(
                descriptionText.rectTransform, eventData.position, eventData.pressEventCamera);

            if (!inDescription)
            {
                ShopSceneManager.Instance.OnCardClicked(this);
                return;
            }

            int charIndex = TMP_TextUtilities.FindIntersectingCharacter(
                descriptionText, eventData.position, eventData.pressEventCamera, true);

            Debug.Log($"[CardView] 클릭 글자 인덱스: {charIndex}");

            // 글자에 히트하지 못했으면 카드 클릭 처리
            if (charIndex == -1)
            {
                ShopSceneManager.Instance.OnCardClicked(this);
                return;
            }

            // 픽 모드가 아니거나, 붙일 스티커가 없으면 카드 클릭 처리
            var sticker = paramPickMode ? pickModeSticker : null;
            if (sticker == null)
            {
                ShopSceneManager.Instance.OnCardClicked(this);
                return;
            }

            // 글자 인덱스 → 파라미터 인덱스
            int newParamIdx = card.FindParamIndexByCharIndex(charIndex);
            if (newParamIdx < 0)
            {
                Debug.LogWarning("[CardView] 파라미터 범위 밖 클릭");
                return;
            }

            // 다른 칸으로 이동하는 경우: 이전 칸 복원
            if (appliedParamIdxForPickMode >= 0 && appliedParamIdxForPickMode != newParamIdx)
            {
                card.RemoveStickerOverridesByInstance(sticker);

                if (previewReplacedBackup.TryGetValue(appliedParamIdxForPickMode, out var prev))
                {
                    card.TryApplyStickerOverrideAtParamIndex(appliedParamIdxForPickMode, prev);
                    previewReplacedBackup.Remove(appliedParamIdxForPickMode);
                }
            }

            // 새 칸 덮어쓰기 전에 기존 스티커 백업(같은 인스턴스가 아니면)
            if (card.stickerOverrides != null &&
                card.stickerOverrides.TryGetValue(newParamIdx, out var existingAtNew) &&
                existingAtNew != null &&
                existingAtNew.instanceId != sticker.instanceId &&
                !previewReplacedBackup.ContainsKey(newParamIdx))
            {
                previewReplacedBackup[newParamIdx] = existingAtNew;
            }

            // 새 칸에 이번 스티커 인스턴스 적용
            bool applied = card.TryApplyStickerOverrideAtParamIndex(newParamIdx, sticker);
            if (applied)
            {
                appliedParamIdxForPickMode = newParamIdx;
                UpdateView();
                onParamPicked?.Invoke(newParamIdx); // Confirm 버튼 활성화
            }
            else
            {
                Debug.LogWarning("[CardView] 스티커 적용 실패: 타입 불일치 또는 불가");
            }
        }


        /// <summary>
        /// 카드가 선택되었는지 시각적 강조
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (!canInteract)
                return;
            
            if (selectionOutline != null)
                selectionOutline.color = selected ? Color.yellow : new Color(246f/255f, 220f/255f, 168f/255f, 1f);
        }

        #endregion
    }
}
