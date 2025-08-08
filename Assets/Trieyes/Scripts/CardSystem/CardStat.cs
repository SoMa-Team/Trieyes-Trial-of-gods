using System.Collections.Generic;
using Stats;
using UnityEngine;

namespace CardSystem
{
    /// <summary>
    /// 스탯 타입과 값을 쌍으로 관리하는 구조체입니다.
    /// 카드의 스탯 정보를 효율적으로 저장하고 관리하기 위해 사용됩니다.
    /// </summary>
    [System.Serializable]
    public struct StatValuePair
    {
        /// <summary>
        /// 스탯의 타입입니다.
        /// </summary>
        public StatType type;

        /// <summary>
        /// 스탯의 값입니다.
        /// IntegerStatValue를 사용하여 버프와 디버프를 지원합니다.
        /// </summary>
        public IntegerStatValue value;

        /// <summary>
        /// StatValuePair의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="type">스탯 타입</param>
        /// <param name="value">스탯 값</param>
        public StatValuePair(StatType type, IntegerStatValue value)
        {
            this.type = type;
            this.value = value;
        }
    }
    /// <summary>
    /// 카드의 속성별 스탯 정보를 관리하는 클래스입니다.
    /// 카드의 속성(Property)과 레벨에 따라 스탯을 계산하고 관리합니다.
    /// 스티커 시스템을 지원하여 동일한 스탯 타입의 여러 값을 효율적으로 처리합니다.
    /// </summary>
    public class CardStat
    {
        // --- 필드 ---

        // ===== [기능 1] 카드 스탯 정보 및 생성 =====
        /// <summary>
        /// 카드의 모든 스탯 정보를 담고 있는 리스트입니다.
        /// StatValuePair 구조체를 사용하여 스탯 타입과 값을 쌍으로 관리합니다.
        /// 스티커를 처리하기 힘들 거 같다라는 얘기가 나와서
        /// 공격력 +10 공격력 +10 -> 공격력 +20
        /// 몇번 인덱스의 인티저밸류스탯밸류에다가 스티커를 적용했는지를 저장
        /// </summary>
        public List<StatValuePair> stats;

        // --- 생성자 ---

        /// <summary>
        /// CardStat의 새 인스턴스를 초기화합니다.
        /// 주어진 속성 배열과 레벨에 따라 스탯을 설정합니다.
        /// </summary>
        /// <param name="properties">카드가 가진 속성들의 배열</param>
        /// <param name="level">카드의 레벨</param>
        public CardStat(Property[] properties, int level)
        {
            stats = new List<StatValuePair>();
            foreach (var property in properties)
            {
                AddStat(property, level);
            }
        }

        // --- public 메서드 ---

        // ===== [기능 2] 속성과 레벨에 따른 스탯 추가=====
        /// <summary>
        /// 속성과 레벨에 따라 스탯 리스트에 스탯을 추가합니다.
        /// 레벨 * 7의 값을 각 속성에 할당하여 카드의 성능을 결정합니다.
        /// </summary>
        /// <param name="property">추가할 스탯의 속성</param>
        /// <param name="level">카드의 레벨</param>
        public void AddStat(Property property, int level)
        {
            Debug.Log($"Add Stat - Card Level: {level}");
            StatType targetStat = GetStatType(property);
            var statValue = new IntegerStatValue(level * 7);
            
            Debug.Log($"Stat Value: {statValue.Value}");
            
            stats.Add(new StatValuePair(targetStat, statValue));
        }
        
        /// <summary>
        /// 특정 스탯 타입의 모든 값의 합계를 반환합니다.
        /// 동일한 스탯 타입의 여러 값들을 합산하여 총합을 계산합니다.
        /// </summary>
        /// <param name="type">계산할 스탯 타입</param>
        /// <returns>해당 스탯 타입의 모든 값의 합계</returns>
        public int GetSumValue(StatType type)
        {
            int sum = 0;
            foreach (var pair in stats)
            {
                if (pair.type == type)
                    sum += pair.value.Value;
            }
            return sum;
        }
        
        /// <summary>
        /// 특정 스탯 타입의 모든 IntegerStatValue 객체들을 반환합니다.
        /// 스티커 시스템에서 개별 스탯 값에 접근할 때 사용됩니다.
        /// </summary>
        /// <param name="type">찾을 스탯 타입</param>
        /// <returns>해당 스탯 타입의 모든 IntegerStatValue 객체들의 리스트</returns>
        public List<IntegerStatValue> GetAllValues(StatType type)
        {
            var list = new List<IntegerStatValue>();
            foreach (var pair in stats)
            {
                if (pair.type == type)
                    list.Add(pair.value);
            }
            return list;
        }

        // --- private 메서드 ---

        /// <summary>
        /// Property 열거형을 StatType으로 변환합니다.
        /// 카드의 속성을 게임 시스템의 스탯 타입으로 매핑합니다.
        /// </summary>
        /// <param name="property">변환할 Property</param>
        /// <returns>대응하는 StatType</returns>
        private StatType GetStatType(Property property)
        {
            switch (property)
            {
                case Property.Fire:
                    return StatType.AttackPower;
                case Property.Steel:
                    return StatType.Defense;
                case Property.Light:
                    return StatType.Health;
                case Property.Dark:
                    return StatType.MoveSpeed;
                case Property.Ice:
                    return StatType.AttackSpeed;
                default:
                    return StatType.CriticalRate;
            }
        }
        
        public CardStat DeepCopy()
        {
            var clone = new CardStat(new Property[0], 0);
            clone.stats = new List<StatValuePair>();
            foreach (var pair in this.stats)
            {
                clone.stats.Add(new StatValuePair(pair.type, pair.value.DeepCopy()));
            }
            return clone;
        }
    }
} 