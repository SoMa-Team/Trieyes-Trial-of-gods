using System.Collections.Generic;
using UnityEngine;
using System;
using CardActions;
using Utils;

namespace CardActions
{
    using CardActionID = Int32;

    /// <summary>
    /// 다양한 CardAction SO를 관리하고 반환하는 팩토리 클래스입니다.
    /// 카드 ID를 통해 CardAction ScriptableObject를 반환합니다.
    /// </summary>
    public class CardActionFactory : MonoBehaviour
    {
        public static CardActionFactory Instance { get; private set; }

        [Header("등록된 카드 액션 SO 리스트")]
        public List<CardAction> cardActionSOs = new(); // SO 리스트로 변경

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        /// <summary>
        /// CardActionID(=인덱스)로 CardAction SO를 반환
        /// </summary>
        public CardAction Create(CardActionID actionId)
        {
            var action = GetCardActionById(actionId);
            if (action == null)
            {
                Debug.LogWarning($"[CardActionFactory] 유효하지 않은 actionId: {actionId}");
            }
            return action;
        }

        /// <summary>
        /// ID에 맞는 CardAction SO 반환
        /// </summary>
        public CardAction GetCardActionById(CardActionID actionId)
        {
            if (actionId < 0 || actionId >= cardActionSOs.Count)
            {
                Debug.LogWarning($"[CardActionFactory] 유효하지 않은 actionId: {actionId}");
                return null;
            }
            return cardActionSOs[actionId];
        }
    }
}
