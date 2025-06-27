using System.Collections.Generic;
using Stats;

namespace CardSystem
{
    [System.Serializable]
    public struct StatValuePair
    {
        public StatType type;
        public IntegerStatValue value;

        public StatValuePair(StatType type, IntegerStatValue value)
        {
            this.type = type;
            this.value = value;
        }
    }
    /// <summary>
    /// 카드의 속성별 스탯 정보를 관리하는 클래스입니다.
    /// 카드의 속성(Property)과 레벨에 따라 스탯을 계산하고 관리합니다.
    /// </summary>
    public class CardStat
    {
        // --- 필드 ---

        // ===== [기능 1] 카드 스탯 정보 및 생성 =====
        /// <summary>
        /// 카드의 모든 스탯 정보를 담고 있는 StatSheet 객체입니다.
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
        /// 속성과 레벨에 따라 스탯 시트에 스탯을 추가합니다.
        /// 현재는 임시 로직으로 구현되어 있으며, 레벨 * 10의 값을 각 속성에 할당합니다.
        /// </summary>
        /// <param name="property">추가할 스탯의 속성</param>
        /// <param name="level">카드의 레벨</param>
        public void AddStat(Property property, int level)
        {
            StatType targetStat = GetStatType(property);
            var statValue = new IntegerStatValue(level * 10);
            
            stats.Add(new StatValuePair(targetStat, statValue));
        }
        
        private StatType GetStatType(Property property)
        {
            switch (property)
            {
                case Property.Attack:
                    return StatType.AttackPower;
                case Property.Defense:
                    return StatType.Defense;
                case Property.Health:
                    return StatType.Health;
                case Property.MoveSpeed:
                    return StatType.MoveSpeed;
                default:
                    return StatType.AttackSpeed;
            }
        }
        
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
    }
} 