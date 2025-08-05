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
        public StatTypeEmblemSO statTypeEmblemTable;
        public Image statTypeEmblemImage;
        public TMP_Text statIntegerValueText;
        public Image selectionOutline;

        public List<GameObject> stickerOverlayPrefabs; // [0]=None, [1]=StatType, [2]=Number
        public List<GameObject> propertyTypeEmblems; // [0]=Fire, [1]=Ice, [2]=Light, [3]=Dark, [4]=Steel
        public List<GameObject> rarityEmblems;

        // ===== [내부 필드] =====
        private Card card;
        private DeckView parentDeckView;
        private readonly List<GameObject> activeStickerOverlays = new();

        // ===== [상수 및 옵션] =====
        Vector2 overlayPadding = new Vector2(15f, 15f);
        Vector2 stickerPadding = new Vector2(0f, 0f);
        const float OverlayXOffset = -5f;
        const float OverlayYOffset = 0f;
        
        const float BGXOffset = 0f;
        const float BGYOffset = 0f;

        private const float StickerOverlayFixedHeight = 58.28125f;
        private const float NumberStickerOverlayYOffset = -5.1719f;
        #region Overlay 생성 및 관리
        private void CreateParamOverlays(
            int paramIdx, Dictionary<int, List<int>> groupCharsByLineNum, TMP_TextInfo textInfo, StickerType stickerType, string paramText = null)
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
        
                Vector2 padding = stickerType == StickerType.None
                    ? overlayPadding
                    : overlayPadding + stickerPadding;
        
                Vector2 overlayOffset = stickerType == StickerType.None
                    ? new Vector2(BGXOffset, BGYOffset)
                    : new Vector2(OverlayXOffset, OverlayYOffset);

                Vector2 overlaySize = new Vector2(
                    width + padding.x,
                    (stickerType == StickerType.None) ? Mathf.Abs(height) + padding.y : StickerOverlayFixedHeight
                );
                Vector2 overlayPos = bl + new Vector3(-padding.x * 0.5f, -padding.y * 0.5f, 0);
                if (stickerType == StickerType.Number)
                {
                    overlayPos.y += NumberStickerOverlayYOffset;
                }
                overlayPos += overlayOffset;
                
                // == 프리팹 Instantiate, StickerView API 사용 ==
                int prefabIndex = (int)stickerType; // StickerType을 index로
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
                    rt.localScale = Vector3.one;
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
                {
                    // paramText가 null이면 빈 문자열
                    stickerView.UpdateText(paramText ?? "", descriptionText);
                }
                activeStickerOverlays.Add(go);
                Debug.Log($@"
                            ==== Sticker Overlay Debug ====
                            paramIdx: {paramIdx}, line: {lineKv.Key}, stickerType: {stickerType}
                            overlayPos: {overlayPos}
                            bl: {bl}, tr: {tr}, width: {width}, height: {height}
                            descRT: anchoredPosition={descRT.anchoredPosition}, localPosition={descRT.localPosition}, position={descRT.position}
                            rt(parent): anchoredPosition={rt.anchoredPosition}, localPosition={rt.localPosition}, position={rt.position}
                            go.parent: {go.transform.parent.name}
                            ");
            }
        }

        private void SyncStickerOverlays()
        {
            foreach (var go in activeStickerOverlays)
                Destroy(go);
            activeStickerOverlays.Clear();

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

                // paramText는 BG만 그릴 때는 null로 전달, 오버레이일 때만 표시
                string paramText = (stickerType != StickerType.None) ? card.GetEffectiveParamTexts()[paramIdx] : null;
                CreateParamOverlays(paramIdx, groupCharsByLineNum, textInfo, stickerType, paramText);
            }
        }

        #endregion

        #region 기본 CardView 로직

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

            for (int i = 0; i < propertyTypeEmblems.Count; i++)
            {
                int flag = 0;
                foreach (var property in card.properties)
                {
                    if (i == (int)property)
                    {
                        flag = 1;
                        break;
                    }
                }

                if (flag==1)
                {
                    propertyTypeEmblems[i].gameObject.SetActive(true);
                }
                else
                {
                    propertyTypeEmblems[i].gameObject.SetActive(false);
                }
            }

            for (int i = 0; i < rarityEmblems.Count; i++)
            {
                if (i == (int)card.rarity)
                {
                    rarityEmblems[i].gameObject.SetActive(true);
                }
                else
                {
                    rarityEmblems[i].gameObject.SetActive(false);   
                }
            }

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

        private Dictionary<int, List<int>> GroupCharsByLineNum(int start, int end, TMP_TextInfo textInfo)
        {
            Dictionary<int, List<int>> res = new();
            for (int i = start; i <= end; i++)
            {
                if (i <= 0 || i >= textInfo.characterCount) continue;
                int lineNum = textInfo.characterInfo[i].lineNumber;
                if (!res.ContainsKey(lineNum))
                    res[lineNum] = new List<int>();
                res[lineNum].Add(i);
            }
            return res;
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

        #endregion
    }
}
