using System.Collections.Generic;
using Stats;

namespace CardSystem
{
    /// <summary>
    /// 카드의 속성별 스탯 정보를 관리하는 클래스
    /// </summary>
    public class CardStat
    {
        // ===== [기능 1] 카드 스탯 정보 및 생성 =====
        public StatSheet statSheet;

        /// <summary>
        /// CardStat의 새 인스턴스를 초기화합니다.
        /// </summary>
        /// <param name="property">이 CardStat이 대표하는 속성</param>
        /// <param name="level">이 CardStat의 레벨. IntegerStatValue 타입.</param>
        public CardStat(Property property, IntegerStatValue level)
        {
            statSheet = new StatSheet();
        }

        // ===== [기능 2] 스탯 정보 설정 =====
        public void AddStat(Property property, IntegerStatValue level){
            //TODO: 속성과 레벨에 따라 스탯 시트에 스탯을 추가하는 로직 구현
            switch(property){
                case Property.Attack:
                    statSheet[StatType.AttackPower].AddToBasicValue(level*10);
                    break;
                case Property.Defense:
                    statSheet[StatType.Defense].AddToBasicValue(level*10);
                    break;
                case Property.Health:
                    statSheet[StatType.Health].AddToBasicValue(level*10);
                    break;
                case Property.MoveSpeed:
                    statSheet[StatType.MoveSpeed].AddToBasicValue(level*10);
                    break;
                default:
                    statSheet[StatType.AttackSpeed].AddToBasicValue(level*10);
                    break;
            }
        }
    }
} 