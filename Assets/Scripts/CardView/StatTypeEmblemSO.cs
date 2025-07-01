using System.Collections;
using UnityEngine;
using Stats;

namespace CardView
{
    /// <summary>
    /// 카드 스탯 타입별 엠블럼(아이콘) 정보를 저장하는 ScriptableObject입니다.
    /// 스탯 타입에 따라 엠블럼 스프라이트를 반환합니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Card/StatTypeEmblemTable")]
    public class StatTypeEmblemSO : ScriptableObject
    {
        /// 스탯 타입과 엠블럼 스프라이트의 쌍을 저장하는 구조체
        [System.Serializable]
        public struct StatTypeEmblemPair
        {
            /// 스탯 타입
            public StatType statType;
            /// 스탯 타입에 해당하는 엠블럼 스프라이트
            public Sprite emblemSprite;
        }

        /// 스탯 타입-엠블럼 쌍 배열
        public StatTypeEmblemPair[] emblems;

        /// <summary>
        /// 주어진 스탯 타입에 해당하는 엠블럼 스프라이트를 반환합니다.
        /// 없으면 null 반환
        /// </summary>
        public Sprite GetEmblem(StatType statType)
        {
            foreach(var pair in emblems)
                if(pair.statType == statType)
                    return pair.emblemSprite;
            return null;
        }
    }
}