using CardActions;
using System.Collections.Generic;
using Utils;
using UnityEngine;
using System;

namespace CardActions
{
    using CardActionID = Int32;

    /// <summary>
    /// 다양한 CardAction 인스턴스를 생성하는 팩토리 클래스입니다.
    /// 각 CardAction은 특정 게임 이벤트에 반응하는 고유한 로직을 캡슐화합니다.
    /// </summary>
    public class CardActionFactory : MonoBehaviour, IFactory<CardAction>
    {
        public static CardActionFactory instance { private set; get; }

        public List<GameObject> cardPrefabs;

        private void Awake()
        {
            if (instance is not null)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
        }

        /// <summary>
        /// CardActionID에 해당하는 CardAction을 생성합니다.
        /// </summary>
        public CardAction Create(CardActionID actionId)
        {
            var action = ClonePrefab(actionId);
            Activate(action);
            return action;
        }

        /// <summary>
        /// CardAction을 활성화합니다.
        /// </summary>
        public void Activate(CardAction action)
        {
            Debug.Log($"CardAction activated! {action}");
            action.Activate();
            // TODO: 풀링 등 추가
        }

        /// <summary>
        /// CardAction을 비활성화합니다.
        /// </summary>
        public void Deactivate(CardAction action)
        {
            action.Deactivate();
            // TODO: 풀링 등 추가
        }

        /// <summary>
        /// 프리팹 복제 및 CardAction 컴포넌트 반환
        /// </summary>
        private CardAction ClonePrefab(CardActionID actionId)
        {
            var prefab = Instantiate(GetPrefabById(actionId));
            var action = prefab.GetComponent<CardAction>();
            if (action == null)
            {
                Debug.LogWarning($"CardAction 컴포넌트가 프리팹에 없습니다. actionId={actionId}");
            }
            return action;
        }

        /// <summary>
        /// ID에 맞는 카드 액션 프리팹 반환
        /// </summary>
        private GameObject GetPrefabById(CardActionID actionId)
        {
            //임시 actionID를 인덱스로 사용하여 반환
            return cardPrefabs[actionId];
        }
    }
}
