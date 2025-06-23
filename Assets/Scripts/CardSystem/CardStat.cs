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

        public CardStat(Property[] properties, int level)
        {
            statSheet = new StatSheet();
            foreach(var property in properties){
                AddStat(property, level);
            }
        }

        // ===== [기능 2] 속성과 레벨에 따른 스탯 추가=====
        public void AddStat(Property property, int level){
            //TODO: 속성과 레벨에 따라 스탯 시트에 스탯을 추가하는 로직 구현
            //아래는 임시 로직
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