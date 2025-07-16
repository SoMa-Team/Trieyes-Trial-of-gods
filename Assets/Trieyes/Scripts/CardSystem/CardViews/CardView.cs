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

        public List<Sprite> stickerBackgroundSprites;

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
        /// 카드 정보에 따라 UI 갱신
        /// </summary>
        public void UpdateView()
        {
            illustrationImage.sprite = card.illustration;
            expFill.fillAmount = (float)card.cardEnhancement.exp.Value / (card.cardEnhancement.level.Value * 10);

            cardNameText.text = card.cardName;
            var descParams = card.GetEffectiveParamTexts();
            descriptionText.text = FormatDescription(card.cardDescription, descParams);
            levelText.text = $"Lv.{card.cardEnhancement.level.Value}";

            // --- 속성 엠블럼 처리 ---
            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = propertyEmblemImage.sprite != null;
            }
            else propertyEmblemImage.enabled = false;

            // --- 스탯 엠블럼 및 값 표시 ---
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
            SyncStickerOverlays();
        }
        
        private void SyncStickerOverlays()
        {
            // 1. 기존 오버레이 제거
            foreach (var go in activeStickerOverlays)
                Destroy(go);
            activeStickerOverlays.Clear();

            // [중요] 텍스트 메쉬 프로 mesh정보 최신화
            descriptionText.ForceMeshUpdate();
            var textInfo = descriptionText.textInfo;

            // 2. 모든 스티커 파라미터(혹은 stickerOverrides.Keys) 순회
            foreach (var kv in card.stickerOverrides)
            {
                int paramIdx = kv.Key;
                Sticker sticker = kv.Value;

                if (paramIdx < 0 || card.paramWordRanges == null || paramIdx >= card.paramWordRanges.Count)
                    continue;
                var range = card.paramWordRanges[paramIdx];

                // 1. 줄별로 해당 파라미터 단어 인덱스 그룹핑
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

                // 2. 각 줄에 대해 오버레이 생성
                foreach (var lineKv in lineToWordIdx)
                {
                    var wordIndices = lineKv.Value;
                    var wordInfoStart = textInfo.wordInfo[wordIndices[0]];
                    var wordInfoEnd = textInfo.wordInfo[wordIndices[^1]];

                    // 좌상단과 우하단 localRect 얻기
                    var localRectStart = wordInfoStart.GetLocalRect(descriptionText);
                    var localRectEnd = wordInfoEnd.GetLocalRect(descriptionText);

                    // 줄별로 하나의 오버레이 박스 계산
                    float left = localRectStart.x;
                    float right = localRectEnd.x + localRectEnd.width;
                    float top = localRectStart.y;
                    float bottom = localRectStart.y - localRectStart.height; // TextMeshPro는 y가 아래로 감소

                    float width = right - left;
                    float height = localRectStart.height; // 한 줄의 높이

                    // 3. 오버레이 이미지 오브젝트 생성
                    var overlayGO = new GameObject($"StickerOverlay_{paramIdx}_line{lineKv.Key}", typeof(UnityEngine.UI.Image));
                    overlayGO.transform.SetParent(descriptionText.transform.parent, false);
                    overlayGO.transform.SetAsFirstSibling();

                    var img = overlayGO.GetComponent<UnityEngine.UI.Image>();
                    int spriteIndex = GetStickerSpriteIndex(sticker.type);
                    img.sprite = stickerBackgroundSprites[spriteIndex];
                    img.color = Color.white;

                    // 4. 이미지 RectTransform 세팅
                    var rt = overlayGO.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1); // 줄의 좌상단에 맞춤
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                    rt.localScale = Vector3.one;
                    rt.localRotation = Quaternion.identity;

                    // 실제 단어 영역보다 살짝 크게(여유 padding) 만들고 싶다면:
                    float padding = 6f; // px단위, 원하는만큼 조절
                    rt.sizeDelta = new Vector2(width + padding, height + padding);
                    rt.anchoredPosition = new Vector2(left - padding * 0.5f, top + padding * 0.5f);

                    activeStickerOverlays.Add(overlayGO);
                }
            }
        }

        
        private int GetStickerSpriteIndex(StickerType type)
        {
            switch (type)
            {
                case StickerType.StatType: return 0;
                case StickerType.Number:   return 1;
                // case StickerType.YourNewType: return 2;
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
                            // (추후 피드백 UI 가능)
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
