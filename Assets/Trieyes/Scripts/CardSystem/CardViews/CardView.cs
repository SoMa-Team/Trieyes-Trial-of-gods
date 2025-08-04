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

        public List<Sprite> stickerBackgroundSprites; // [0]=None, [1]=StatType, [2]=Number

        // ===== [내부 필드] =====
        private Card card;
        private DeckView parentDeckView;
        private readonly List<GameObject> activeStickerOverlays = new();

        // ===== [상수 및 옵션] =====
        Vector2 overlayPadding = new Vector2(15f, 40f);
        Vector2 stickerPadding = new Vector2(5f, 15f);
        const float TextXOffset = 6f;
        const float TextYOffset = -8f;
        const float OverlayXOffset = 0f;
        const float OverlayYOffset = 0f;

        private static readonly Color BgColorDefault = Color.white;
        private static readonly Color TextColorDefault = Color.black;
        
        private static readonly Color NoneStickerColor = new Color(137/255f, 137/255f, 137/255f, 1f); // R, G, B, A
        private static readonly Color StickerColor = new Color(171/255f, 205/255f, 239/255f, 1f); // R, G, B, A


        private enum OverlayLayer
        {
            UnderDescription,      // descriptionText의 sibling 뒤 (BG만)
            OverDescriptionBG,     // descriptionText의 자식, BG
            OverDescriptionText    // descriptionText의 자식, Text
        }

        #region Overlay 생성 및 관리

        private GameObject CreateOverlayObject(
            Vector2 anchoredPos, Vector2 size, string objName,
            OverlayLayer layer,
            Sprite sprite = null, Color? bgColor = null,
            string text = null, TMP_FontAsset font = null, float? fontSize = null, Color? textColor = null)
        {
            GameObject go;

            if (text == null)
            {
                go = new GameObject(objName, typeof(Image));
                var img = go.GetComponent<Image>();
                img.sprite = sprite;
                img.color = bgColor ?? BgColorDefault;
            }
            else
            {
                go = new GameObject(objName, typeof(TextMeshProUGUI));
                var tmp = go.GetComponent<TextMeshProUGUI>();
                tmp.text = text;
                tmp.font = descriptionText.font;
                tmp.fontSize = descriptionText.fontSize;
                tmp.fontStyle = descriptionText.fontStyle;
                tmp.lineSpacing = descriptionText.lineSpacing;
                tmp.fontWeight = descriptionText.fontWeight;
                tmp.alignment = descriptionText.alignment;
                tmp.enableAutoSizing = descriptionText.enableAutoSizing;
                tmp.richText = descriptionText.richText;
                tmp.characterSpacing = descriptionText.characterSpacing;
                tmp.wordSpacing = descriptionText.wordSpacing;
                tmp.paragraphSpacing = descriptionText.paragraphSpacing;
                tmp.color = textColor ?? TextColorDefault;
                tmp.raycastTarget = false;
                go.transform.localScale = Vector3.one;
            }
            
            var descRT = descriptionText.rectTransform;
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.localScale = Vector3.one;

            if (layer == OverlayLayer.UnderDescription)
            {
                // 부모를 descriptionText의 부모로!
                var parentRT = descRT.parent as RectTransform;
                go.transform.SetParent(parentRT, false);
                go.transform.SetAsFirstSibling();

                // anchor/pivot 모두 부모의 구조와 동일하게!
                rt.anchorMin = parentRT.anchorMin;
                rt.anchorMax = parentRT.anchorMax;
                rt.pivot = parentRT.pivot;

                // World 좌표 변환 → 부모 로컬 기준 anchPos 보정
                Vector3 worldPos = descRT.TransformPoint(anchoredPos);
                Vector3 localPos = parentRT.InverseTransformPoint(worldPos);
                rt.anchoredPosition = new Vector2(localPos.x, localPos.y);
            }
            else
            {
                // descriptionText를 부모로!
                go.transform.SetParent(descRT, false);
                go.transform.SetAsLastSibling();

                // anchor/pivot descriptionText와 동일하게!
                rt.anchorMin = descRT.anchorMin;
                rt.anchorMax = descRT.anchorMax;
                rt.pivot = descRT.pivot;

                // anchPos 그대로 사용
                rt.anchoredPosition = anchoredPos;
            }

            return go;
        }


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
                    ? new Vector2(OverlayXOffset, OverlayYOffset)
                    : new Vector2(OverlayXOffset - 5f, OverlayYOffset);

                Vector2 overlaySize = new Vector2(
                    width + padding.x,
                    Mathf.Abs(height) + padding.y
                );
                Vector2 overlayPos = bl + new Vector3(-padding.x * 0.5f, -padding.y * 0.5f, 0);
                
                overlayPos += overlayOffset;

                if (stickerType == StickerType.None)
                {
                    // BG만 descriptionText sibling 뒤에 배치
                    var bg = CreateOverlayObject(
                        overlayPos, overlaySize, $"StickerBG_None_{paramIdx}_line{lineKv.Key}",
                        OverlayLayer.UnderDescription,
                        sprite: stickerBackgroundSprites[(int)StickerType.None],
                        NoneStickerColor
                    );
                    activeStickerOverlays.Add(bg);
                }
                else
                {
                    // BG 오버레이 (descriptionText 자식)
                    var bg = CreateOverlayObject(
                        overlayPos, overlaySize, $"StickerOverlay_{paramIdx}_line{lineKv.Key}",
                        OverlayLayer.OverDescriptionBG,
                        sprite: stickerBackgroundSprites[(int)stickerType],
                        StickerColor
                    );
                    activeStickerOverlays.Add(bg);
                    
                    Vector2 textOffset = new Vector2(TextXOffset,  TextYOffset);

                    // paramText TMP 오버레이 (BG 위)
                    var txt = CreateOverlayObject(
                        overlayPos+textOffset, overlaySize, $"StickerText_{paramIdx}_line{lineKv.Key}",
                        OverlayLayer.OverDescriptionText,
                        text: paramText,
                        font: descriptionText.font,
                        fontSize: descriptionText.fontSize
                    );
                    activeStickerOverlays.Add(txt);
                }
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
