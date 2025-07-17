using UnityEngine;
using System.Collections.Generic;
using Stats;
using CardActions;
using System;
using CharacterSystem;
using StickerSystem;
using Utils;
using System.Linq;

namespace CardSystem
{
    /// <summary>
    /// 카드의 고유 식별자를 위한 타입 별칭입니다.
    /// </summary>
    using CardID = Int32;

    /// <summary>
    /// 카드의 기본 정보를 담고, 파라미터 치환, 스티커 적용 등 
    /// 카드의 핵심 데이터와 기능을 관리하는 클래스입니다.
    /// CardInfo와 CardAction을 분리하여 데이터와 로직을 명확히 구분합니다.
    /// </summary>
    public class Card
    {
        // ==== [필드] ====
        
        /// <summary> 카드 ID 생성을 위한 정적 카운터 </summary>
        private static int idCounter = 0;

        [Header("Card Info")]
        /// <summary> 카드의 고유 식별자 (자동 증가) </summary>
        public CardID cardId;

        /// <summary> 카드의 속성 (공격/방어/속도 등) </summary>
        public Property[] properties;

        /// <summary> 카드의 희귀도 </summary>
        public Rarity rarity;

        /// <summary> 카드명 </summary>
        public string cardName;

        /// <summary> 카드 일러스트 이미지 </summary>
        public Sprite illustration;

        /// <summary> 카드 설명 (치환 파라미터 포함) </summary>
        [TextArea] public string cardDescription;

        /// <summary> 카드가 반응하는 이벤트 타입 리스트 </summary>
        public List<Utils.EventType> eventTypes = new();

        /// <summary> 카드 액션(행동/효과) </summary>
        public CardAction cardAction;

        /// <summary> 카드의 현재 스탯 </summary>
        public CardStat cardStats;

        /// <summary> 카드의 강화 정보 (레벨/경험치) </summary>
        public CardEnhancement cardEnhancement;

        /// <summary> 각 파라미터(치환값)별 스티커 오버라이드 맵 </summary>
        public Dictionary<int, Sticker> stickerOverrides = new();

        /// <summary> 카드 파라미터의 원본 값 리스트 </summary>
        public List<string> baseParams;

        /// <summary> 각 파라미터가 descriptionText에서 차지하는 [시작, 끝] 단어 인덱스 리스트 </summary>
        public List<ParamWordRange> paramWordRanges;

        /// <summary> 카드 소유자 </summary>
        private Pawn owner;

        // ==== [생성자] ====
        public Card()
        {
            this.cardId = idCounter++;
        }

        /// <summary>
        /// 카드에 액션을 할당 (내부 참조까지 세팅)
        /// </summary>
        public void SetCardAction(CardAction action)
        {
            cardAction = action;
            cardAction.SetCard(this);
        }

        // ==== [카드 기본 기능] ====

        /// <summary>
        /// 카드를 활성화(초기화)하고 스탯/강화 정보를 준비합니다.
        /// </summary>
        public void Activate(int level)
        {
            Debug.Log($"Card Activated! {cardId}, card level: {level}");
            cardStats = new CardStat(properties, level);
            cardEnhancement = new CardEnhancement(level, 0);
        }

        /// <summary>
        /// 카드 비활성화 (리소스 해제 등)
        /// </summary>
        public void Deactivate()
        {
            // TODO: 필요시 구현
        }

        /// <summary>
        /// 카드 이벤트 트리거 (이벤트 발생시 효과 발동)
        /// </summary>
        public void TriggerCardEvent(Utils.EventType eventType, CardSystem.Deck deck, object param = null)
        {
            cardAction?.OnEvent(owner, deck, eventType, param);
        }

        /// <summary>
        /// 카드의 소유자(적용 대상) 설정
        /// </summary>
        public void SetOwner(CharacterSystem.Pawn pawn)
        {
            owner = pawn;
        }

        /// <summary>
        /// 카드 레벨업 및 스탯 갱신
        /// </summary>
        public void LevelUp()
        {
            cardEnhancement.level.AddToBasicValue(1);
            RefreshStats();
        }

        /// <summary>
        /// 카드의 현재 스탯을 새로 고침
        /// </summary>
        public void RefreshStats()
        {
            cardStats = new CardStat(properties, cardEnhancement.level.Value);
        }

        // ==== [파라미터-스티커 시스템] ====

        /// <summary>
        /// 실제로 치환될 파라미터 텍스트 리스트를 반환 (스티커 적용 반영)
        /// </summary>
        public List<string> GetEffectiveParamTexts()
        {
            var result = new List<string>();
            for (int i = 0; i < baseParams.Count; i++)
            {
                if (stickerOverrides.TryGetValue(i, out var sticker))
                {
                    // 스티커로 오버라이드
                    switch (sticker.type)
                    {
                        case StickerType.StatType:
                            result.Add(StatTypeTransformer.StatTypeToKorean(sticker.statTypeValue));
                            break;
                        case StickerType.Number:
                            result.Add(sticker.numberValue.ToString());
                            break;
                        default:
                            result.Add(""); // 예외 대응
                            break;
                    }
                }
                else
                {
                    if (cardAction == null)
                    {
                        Debug.LogError("cardAction is null in GetEffectiveParamTexts()");
                    }
                    else
                    {
                        var kind = cardAction.GetParamDef(i).kind;
                        var value = cardAction.GetBaseParam(i);
                        if (kind == ParamKind.StatType)
                            result.Add(StatTypeTransformer.StatTypeToKorean((StatType)value));
                        else
                            result.Add(value.ToString());
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// (스티커/파라미터 적용 후) 각 파라미터가 차지하는 단어 범위를 다시 계산
        /// </summary>
        public void RefreshParamWordRanges()
        {
            var values = GetEffectiveParamTexts();
            var wordCounts = values
                .ConvertAll(v => string.IsNullOrWhiteSpace(v) ? 1 : v.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length);

            // paramWordRanges를 깊은 복사 후, 오프셋 계산
            var newRanges = paramWordRanges.Select(r => new ParamWordRange { start = r.start, end = r.end }).ToList();
            UpdateParamWordRanges(newRanges, wordCounts);
            paramWordRanges = newRanges;
        }

        /// <summary>
        /// paramWordRanges 리스트를 파라미터 단어 수에 따라 갱신합니다.
        /// </summary>
        public static void UpdateParamWordRanges(List<ParamWordRange> ranges, List<int> paramWordCounts)
        {
            int offset = 0;
            for (int i = 0; i < ranges.Count; i++)
            {
                int oldCount = ranges[i].end - ranges[i].start + 1;
                int newCount = paramWordCounts[i];
                ranges[i].start += offset;
                ranges[i].end = ranges[i].start + newCount - 1;
                offset += newCount - oldCount;
            }
        }

        /// <summary>
        /// descriptionText의 특정 단어 인덱스에 스티커를 적용
        /// </summary>
        public bool TryApplyStickerOverride(int paramWordIndex, Sticker sticker)
        {
            if (paramWordRanges == null || paramWordRanges.Count == 0)
                return false;
            // paramWordIndex(단어 인덱스)가 어느 파라미터 범위에 속하는지 탐색
            int baseParamIdx = paramWordRanges.FindIndex(r => r.start <= paramWordIndex && paramWordIndex <= r.end);
            if (baseParamIdx == -1)
                return false;

            // 스티커 타입과 파라미터 타입 일치 여부 확인
            var paramKind = cardAction.GetParamDef(baseParamIdx).kind;
            if ((paramKind == ParamKind.Number && sticker.type != StickerType.Number) ||
                (paramKind == ParamKind.StatType && sticker.type != StickerType.StatType))
                return false;

            // 적용
            stickerOverrides[baseParamIdx] = sticker;
            RefreshParamWordRanges();
            return true;
        }

        /// <summary>
        /// Card 객체를 깊은 복사하여 반환
        /// </summary>
        public Card DeepCopy()
        {
            var clone = new Card
            {
                properties = (Property[])this.properties.Clone(),
                rarity = this.rarity,
                cardName = this.cardName,
                illustration = this.illustration,
                cardDescription = this.cardDescription,
                eventTypes = new List<Utils.EventType>(this.eventTypes),
                baseParams = this.baseParams,
                paramWordRanges = this.paramWordRanges != null ? new List<ParamWordRange>(this.paramWordRanges) : new List<ParamWordRange>(),
                cardAction = this.cardAction?.DeepCopy(),
                cardStats = this.cardStats?.DeepCopy(),
                cardEnhancement = this.cardEnhancement?.DeepCopy()
            };
            if(clone.cardAction != null) clone.cardAction.SetCard(clone);
            return clone;
        }
    }
}
