using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CardSystem;
using UnityEngine.EventSystems;
using DeckViews;
using System.Collections.Generic;
using StickerSystem;
using Utils;

namespace CardViews
{
    /// <summary>
    /// 카드의 정보를 UI에 표시하고, 선택/클릭/강조/스티커 적용까지 처리하는 뷰 클래스.
    /// </summary>
    public class CardView : MonoBehaviour, IPointerClickHandler
    {
        // ===== [UI 필드] =====
        public Image illustrationImage;
        public Image expFill;
        public TMP_Text cardNameText;
        public TMP_Text levelText;
        public TMP_Text descriptionText;
        public Image propertyEmblemImage;
        public PropertyEmblemSO propertyEmblemTable;
        public StatTypeEmblemSO statTypeEmblemTable;
        public Image statTypeEmblemImage;
        public TMP_Text statIntegerValueText;
        public Image selectionOutline;

        public List<Sprite> stickerBackgroundSprites; // [0]=StatType, [1]=Number

        // ===== [내부 필드] =====
        private Card card;
        private DeckView parentDeckView;
        private readonly List<GameObject> activeStickerOverlays = new();

        /// <summary>
        /// 상위 덱 뷰 참조 연결
        /// </summary>
        public void SetParentDeckView(DeckViews.DeckView deckView) => parentDeckView = deckView;

        /// <summary>
        /// 카드 정보 할당 및 UI 초기화
        /// </summary>
        public virtual void SetCard(Card card)
        {
            this.card = card;
            SetSelected(false);
            UpdateView();
        }

        /// <summary>
        /// 현재 할당된 카드 반환
        /// </summary>
        public Card GetCurrentCard()
        {
            if (card == null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return card;
        }

        /// <summary>
        /// 카드 정보에 따라 UI 전체 갱신
        /// </summary>
        public void UpdateView()
        {
            // 기본 정보/스탯 등 UI 반영
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);

            cardNameText.text = card.cardName;
            levelText.text = $"Lv.{card.cardEnhancement.level.Value}";

            // 카드 설명의 파라미터 값 적용
            var descParams = card.GetEffectiveParamTexts();
            descriptionText.text = FormatDescription(card.cardDescription, descParams);

            // 속성 엠블럼 처리
            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = propertyEmblemImage.sprite != null;
            }
            else propertyEmblemImage.enabled = false;

            // 스탯 엠블럼 및 값 표시
            if (card.cardStats.stats.Count > 0 && statTypeEmblemTable != null)
            {
                var stat = card.cardStats.stats[0];
                statTypeEmblemImage.sprite = statTypeEmblemTable.GetEmblem(stat.type);
                statTypeEmblemImage.enabled = statTypeEmblemImage.sprite != null;
                statIntegerValueText.text = $"+{stat.value.Value}";
                statIntegerValueText.enabled = true;
            }
            else
            {
                statTypeEmblemImage.enabled = false;
                statIntegerValueText.enabled = false;
            }

            // --- 스티커 오버레이 ---
            SyncStickerOverlays();
        }

        /// <summary>
        /// 카드 설명 내 치환 파라미터 단어에 스티커 배경 오버레이 이미지를 생성/배치
        /// </summary>
        private void SyncStickerOverlays()
        {
            // 1. 기존 오버레이 제거
            foreach (var go in activeStickerOverlays)
                Destroy(go);
            activeStickerOverlays.Clear();

            // 2. 최신 텍스트 mesh 정보 확보
            descriptionText.ForceMeshUpdate();
            var textInfo = descriptionText.textInfo;

            // 3. 각 스티커 파라미터마다 줄 단위로 오버레이를 생성
            foreach (var kv in card.stickerOverrides)
            {
                int paramIdx = kv.Key;
                Sticker sticker = kv.Value;

                if (paramIdx < 0 || card.paramWordRanges == null || paramIdx >= card.paramWordRanges.Count)
                    continue;
                var range = card.paramWordRanges[paramIdx];

                // [핵심] 단어 인덱스 → 줄(line) 단위 그룹핑
                Dictionary<int, List<int>> lineToWordIdx = new();
                for (int wordIdx = range.start; wordIdx <= range.end; wordIdx++)
                {
                    if (wordIdx < 0 || wordIdx >= textInfo.wordCount)
                        continue;
                    int firstCharIdx = textInfo.wordInfo[wordIdx].firstCharacterIndex;
                    if (firstCharIdx < 0 || firstCharIdx >= textInfo.characterCount)
                        continue;
                    int lineNum = textInfo.characterInfo[firstCharIdx].lineNumber;
                    if (!lineToWordIdx.ContainsKey(lineNum))
                        lineToWordIdx[lineNum] = new List<int>();
                    lineToWordIdx[lineNum].Add(wordIdx);
                }

                // 4. 줄별로 나눈 그룹 당 해당 그룹의 배경이 될 하나의 오버레이 박스를 생성
                foreach (var lineKv in lineToWordIdx)
                {
                    var wordIndices = lineKv.Value;
                    var wordInfoStart = textInfo.wordInfo[wordIndices[0]];
                    var wordInfoEnd = textInfo.wordInfo[wordIndices[^1]];

                    // 각 줄의 "첫 단어"와 "마지막 단어"의 localRect(좌표+크기)
                    var localRectStart = wordInfoStart.GetLocalRect(descriptionText);
                    var localRectEnd = wordInfoEnd.GetLocalRect(descriptionText);

                    // (줄 내 여러 단어를 커버하는) 오버레이 박스 크기/좌표 계산
                    float left = localRectStart.x;
                    float right = localRectEnd.x + localRectEnd.width;
                    float top = localRectStart.y;
                    float height = localRectStart.height;
                    float width = right - left;

                    // 5. 실제 오버레이 이미지 오브젝트 생성 및 배치
                    var overlayGO = new GameObject($"StickerOverlay_{paramIdx}_line{lineKv.Key}", typeof(UnityEngine.UI.Image));
                    overlayGO.transform.SetParent(descriptionText.transform.parent, false);
                    overlayGO.transform.SetAsFirstSibling(); // 텍스트 뒤에 렌더

                    var img = overlayGO.GetComponent<UnityEngine.UI.Image>();
                    img.sprite = stickerBackgroundSprites[GetStickerSpriteIndex(sticker.type)];
                    img.color = Color.white;

                    // RectTransform 세팅 (좌상단 anchor/pivot)
                    var rt = overlayGO.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1);
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                    rt.localScale = Vector3.one;
                    rt.localRotation = Quaternion.identity;

                    // padding은 여유롭게(글자가 꽉 차보이지 않게)
                    float padding = 6f;
                    rt.sizeDelta = new Vector2(width + padding, height + padding);
                    rt.anchoredPosition = new Vector2(left - padding * 0.5f, top + padding * 0.5f);

                    activeStickerOverlays.Add(overlayGO);
                }
            }
        }

        /// <summary>
        /// 스티커 타입별 배경 이미지 인덱스 반환
        /// </summary>
        private int GetStickerSpriteIndex(StickerType type)
        {
            switch (type)
            {
                case StickerType.StatType: return 0;
                case StickerType.Number:   return 1;
                default: return 0;
            }
        }

        /// <summary>
        /// 카드 설명(템플릿)에 실제 파라미터 값을 대입해 반환
        /// </summary>
        private string FormatDescription(string template, List<string> descParams)
        {
            if (descParams == null || descParams.Count == 0)
                return template;
            string result = template;
            for (int i = 0; i < descParams.Count; i++)
                result = result.Replace("{" + i + "}", descParams[i]);
            return result;
        }

        /// <summary>
        /// 카드 설명 클릭 시 단어 인덱스 계산 & 스티커 적용 시도.
        /// 아니면 부모 덱 뷰로 카드 클릭 알림.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            // 설명 영역 클릭 시만 처리
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    descriptionText.rectTransform, eventData.position, eventData.pressEventCamera))
            {
                int wordIndex = TMP_TextUtilities.FindIntersectingWord(
                    descriptionText, eventData.position, eventData.pressEventCamera);

                Debug.Log($"[CardView] 클릭 단어 인덱스: {wordIndex}");

                if (wordIndex != -1)
                {
                    var sticker = ShopSceneManager.Instance?.selectedSticker;
                    if (sticker != null)
                    {
                        bool applied = card.TryApplyStickerOverride(wordIndex, sticker);
                        if (applied)
                        {
                            UpdateView();
                            Debug.Log($"[CardView] 스티커가 {wordIndex}번째 파라미터에 적용됨");
                        }
                        else
                        {
                            Debug.LogWarning("[CardView] 스티커 적용 실패: 타입 불일치 또는 불가");
                        }
                        return;
                    }
                    // 스티커 없으면 아래로 Fall-through
                }
            }
            parentDeckView?.OnCardClicked(this); // 카드 선택 등 기존 처리
        }

        /// <summary>
        /// 카드가 선택 상태면 강조, 아니면 기본 색
        /// </summary>
        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
                selectionOutline.color = selected ? Color.yellow : Color.black;
        }
    }
}
