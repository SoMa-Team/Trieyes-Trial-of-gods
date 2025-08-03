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
        public TMP_Text cardNameText;
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

        public void SetParentDeckView(DeckViews.DeckView deckView) => parentDeckView = deckView;

        public virtual void SetCard(Card card)
        {
            this.card = card;
            SetSelected(false);
            UpdateView();
        }

        public Card GetCurrentCard()
        {
            if (card == null) Debug.LogError("CardView.GetCurrentCard: card is null");
            return card;
        }

        public void UpdateView()
        {
            illustrationImage.sprite = card.illustration;

            cardNameText.text = card.cardName;
            if (card.cardEnhancement.level.Value > 1)
            {
                int plusLevel = card.cardEnhancement.level.Value - 1;
                cardNameText.text += $" <color=#FFD600>+{plusLevel}</color>";
            }

            var descParams = card.GetEffectiveParamTexts();
            descriptionText.text = FormatDescription(card.cardDescription, descParams);

            if (card.properties != null && card.properties.Length > 0 && propertyEmblemTable != null)
            {
                propertyEmblemImage.sprite = propertyEmblemTable.GetEmblem(card.properties[0]);
                propertyEmblemImage.enabled = propertyEmblemImage.sprite != null;
            }
            else propertyEmblemImage.enabled = false;

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

        /// <summary>
        /// 카드 설명 내 치환 파라미터(글자 범위)에 스티커 배경 오버레이 이미지를 생성/배치
        /// </summary>
        private void SyncStickerOverlays()
        {
            foreach (var go in activeStickerOverlays)
                Destroy(go);
            activeStickerOverlays.Clear();

            descriptionText.ForceMeshUpdate();
            var textInfo = descriptionText.textInfo;

            foreach (var kv in card.stickerOverrides)
            {
                int paramIdx = kv.Key;
                Sticker sticker = kv.Value;

                if (paramIdx < 0 || card.paramCharRanges == null || paramIdx >= card.paramCharRanges.Count)
                    continue;
                var range = card.paramCharRanges[paramIdx];

                // 줄(line) 단위로 그룹핑
                Dictionary<int, List<int>> lineToCharIdx = new();
                for (int charIdx = range.start; charIdx <= range.end; charIdx++)
                {
                    if (charIdx < 0 || charIdx >= textInfo.characterCount)
                        continue;
                    int lineNum = textInfo.characterInfo[charIdx].lineNumber;
                    if (!lineToCharIdx.ContainsKey(lineNum))
                        lineToCharIdx[lineNum] = new List<int>();
                    lineToCharIdx[lineNum].Add(charIdx);
                }

                foreach (var lineKv in lineToCharIdx)
                {
                    var charIndices = lineKv.Value;
                    var firstCharInfo = textInfo.characterInfo[charIndices[0]];
                    var lastCharInfo = textInfo.characterInfo[charIndices[^1]];

                    Vector3 bl = firstCharInfo.bottomLeft;
                    Vector3 tr = lastCharInfo.topRight;
                    float width = tr.x - bl.x;
                    float height = tr.y - bl.y;

                    var overlayGO = new GameObject($"StickerOverlay_{paramIdx}_line{lineKv.Key}", typeof(UnityEngine.UI.Image));
                    overlayGO.transform.SetParent(descriptionText.transform.parent, false);
                    overlayGO.transform.SetAsFirstSibling();

                    var img = overlayGO.GetComponent<UnityEngine.UI.Image>();
                    img.sprite = stickerBackgroundSprites[(int)sticker.type];
                    img.color = Color.white;

                    var rt = overlayGO.GetComponent<RectTransform>();
                    rt.pivot = new Vector2(0, 1);
                    rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                    rt.localScale = Vector3.one;
                    rt.localRotation = Quaternion.identity;

                    float padding = 6f;
                    rt.sizeDelta = new Vector2(width + padding, Mathf.Abs(height) + padding);
                    rt.anchoredPosition = new Vector2(bl.x - padding * 0.5f, bl.y + Mathf.Abs(height) + padding * 0.5f);

                    activeStickerOverlays.Add(overlayGO);
                }
            }
        }

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
        /// 카드 설명 클릭 시 글자 인덱스 계산 & 스티커 적용 시도.
        /// 아니면 부모 덱 뷰로 카드 클릭 알림.
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                    descriptionText.rectTransform, eventData.position, eventData.pressEventCamera))
            {
                int charIndex = TMP_TextUtilities.FindIntersectingCharacter(
                    descriptionText, eventData.position, eventData.pressEventCamera, true);

                Debug.Log($"[CardView] 클릭 글자 인덱스: {charIndex}");

                if (charIndex != -1)
                {
                    var sticker = ShopSceneManager.Instance?.selectedSticker;
                    if (sticker != null)
                    {
                        bool applied = card.TryApplyStickerOverride(charIndex, sticker);
                        if (applied)
                        {
                            UpdateView();
                            Debug.Log($"[CardView] 스티커가 char {charIndex}번째 파라미터에 적용됨");
                        }
                        else
                        {
                            Debug.LogWarning("[CardView] 스티커 적용 실패: 타입 불일치 또는 불가");
                        }
                        return;
                    }
                }
            }
            parentDeckView?.OnCardClicked(this);
        }

        public void SetSelected(bool selected)
        {
            if (selectionOutline != null)
                selectionOutline.color = selected ? Color.yellow : Color.black;
        }
    }
}
