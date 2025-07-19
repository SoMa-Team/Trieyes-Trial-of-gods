// using Utils;
// using System.Collections.Generic;
// using UnityEngine;
// using Stats;
// using CharacterSystem;
// using CardSystem;
// using System;
//
// namespace CardActions
// {
//     public class Card0301_DestroyRightAndAbsorbAction : CardAction
// {
//     private const int statTypeIndex1 = 0; // 피해흡혈
//     private const int statTypeIndex2 = 1; // 공격력
//     private const int percentIndex   = 2; // 증가 퍼센트 (기본 30)
//
//     public Card0301_DestroyRightAndAbsorbAction()
//     {
//         actionParams = new List<ActionParam>
//         {
//             // 피해흡혈(StatType)
//             ActionParamFactory.Create(ParamKind.StatType, card =>
//                 StatTypeTransformer.KoreanToStatType(card.baseParams[statTypeIndex1])
//             ),
//             // 공격력(StatType)
//             ActionParamFactory.Create(ParamKind.StatType, card =>
//                 StatTypeTransformer.KoreanToStatType(card.baseParams[statTypeIndex2])
//             ),
//             // 증가 퍼센트 (레벨 반영)
//             ActionParamFactory.Create(ParamKind.Number, card =>
//             {
//                 string raw = card.baseParams[percentIndex];
//                 int basePercent = int.Parse(raw);
//                 // 예시: 30 + 10 * 레벨
//                 return basePercent + 10 * card.cardEnhancement.level.Value;
//             }),
//         };
//     }
//
//     public override void OnEvent(Pawn owner, Deck deck, EventType eventType, object param)
//     {
//         if (eventType != EventType.DestoryCardsBeforeBattleStart)
//             return;
//         if (!(param is int myIdx))
//         {
//             Debug.LogError("카드 인덱스 param이 int가 아님!");
//             return;
//         }
//         if (myIdx < deck.Cards.Count - 1)
//         {
//             var rightCard = deck.Cards[myIdx + 1];
//             int destroyedLevel = rightCard.cardEnhancement.level.Value;
//             deck.RemoveCard(rightCard);
//             Debug.Log($"오른쪽 카드 {rightCard.cardName} (레벨 {destroyedLevel}) 파괴됨");
//
//             // 경험치 스택 추가
//             card.cardEnhancement.AddExp(destroyedLevel);
//             card.RefreshStats();
//         }
//         else
//         {
//             Debug.Log("오른쪽 카드가 없음 (파괴 불가)");
//         }
//
//         // 파라미터 치환값 적용
//         var statType1 = (StatType)GetEffectiveParam(statTypeIndex1);
//         var statType2 = (StatType)GetEffectiveParam(statTypeIndex2);
//         int percent = Convert.ToInt32(GetEffectiveParam(percentIndex));
//
//         AddPercentBuff(owner, statType1, percent);
//         AddPercentBuff(owner, statType2, percent);
//     }
//
//     private void AddPercentBuff(Pawn owner, StatType stat, int percent)
//     {
//         var baseValue = owner.statSheet[stat].BaseValue;
//         int value = Mathf.RoundToInt(baseValue * (percent / 100f));
//         var buff = new StatModifier(value, BuffOperationType.Additive);
//         owner.statSheet[stat].AddBuff(buff);
//         Debug.Log($"{stat}: {percent}% -> +{value} 버프");
//     }
// }
//
// }