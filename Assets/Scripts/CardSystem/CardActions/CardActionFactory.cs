using System.Collections.Generic;
using UnityEngine;
using System;
using CardActions;
using Utils;

namespace CardActions
{
    /// <summary>
    /// 카드 액션의 고유 식별자를 위한 타입 별칭입니다.
    /// </summary>
    using CardActionID = Int32;

    /// <summary>
    /// 다양한 CardAction ScriptableObject를 관리하고 반환하는 팩토리 클래스입니다.
    /// 카드 ID를 통해 CardAction ScriptableObject를 반환하며, 싱글톤 패턴을 사용합니다.
    /// </summary>
    public class CardActionFactory : MonoBehaviour
    {
        // --- 필드 ---

        /// <summary>
        /// CardActionFactory의 싱글톤 인스턴스입니다.
        /// </summary>
        public static CardActionFactory Instance { get; private set; }

        [Header("등록된 카드 액션 SO 리스트")]
        /// <summary>
        /// 등록된 모든 CardAction ScriptableObject들의 리스트입니다.
        /// 인덱스가 CardActionID로 사용됩니다.
        /// </summary>
        public List<CardAction> cardActionSOs = new();

        // --- private 메서드 ---

        /// <summary>
        /// MonoBehaviour의 Awake 메서드입니다.
        /// 싱글톤 패턴을 구현하여 중복 인스턴스를 방지합니다.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        // --- public 메서드 ---

        /// <summary>
        /// CardActionID(=인덱스)로 CardAction ScriptableObject를 반환합니다.
        /// </summary>
        /// <param name="actionId">반환할 CardAction의 ID</param>
        /// <returns>해당 ID의 CardAction ScriptableObject</returns>
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
        /// ID에 맞는 CardAction ScriptableObject를 반환합니다.
        /// 유효하지 않은 ID인 경우 null을 반환합니다.
        /// </summary>
        /// <param name="actionId">찾을 CardAction의 ID</param>
        /// <returns>해당 ID의 CardAction ScriptableObject 또는 null</returns>
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
