using System.Collections.Generic;
using CardSystem;
using Stats;

namespace Utils
{
    public enum TriggerType
    {
        Card,
        Stat
    }
    
    public class TriggerInfo
    {
        public TriggerType type;
        
        #nullable enable
        public CardTriggerInfo? cardTriggerInfo;
        public StatTriggerInfo? statTriggerInfo;
        #nullable disable

        public static TriggerInfo MakeCardInfo(int index, Card card)
        {
            return new TriggerInfo
            {
                type = TriggerType.Card,
                cardTriggerInfo = new CardTriggerInfo
                {
                    index = index,
                    card = card
                },
                statTriggerInfo = null
            };
        }

        public static TriggerInfo MakeStatInfo(StatType statType, StatModifier modifier)
        {
            return new TriggerInfo
            {
                type = TriggerType.Stat,
                cardTriggerInfo = null,
                statTriggerInfo = new StatTriggerInfo
                {
                    statType = statType,
                    modifier = modifier
                }
            };
        }
    }
    
    public class CardTriggerInfo
    {
        public int index;
        public Card card;
    }
    
    public class StatTriggerInfo
    {
        public StatType statType;
        public StatModifier modifier;
    }
    
    public class CardStatChangeRecorder
    {
        private static CardStatChangeRecorder _instance;
        public static CardStatChangeRecorder Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new CardStatChangeRecorder();
                return _instance;
            }
        }

        private bool isActivate = false;
        private Queue<TriggerInfo> triggers = new();

        private StatType lastStatType;

        public void RecordStart()
        {
            isActivate = true;
        }

        public void AddCardTrigger(int index, Card card)
        {
            if (!isActivate)
                return;
            triggers.Enqueue(TriggerInfo.MakeCardInfo(index, card));
        }

        public void AddStatTrigger(StatType statType)
        {
            lastStatType = statType;
        }

        public void AddStatValueTrigger(StatModifier modifier)
        {
            if (!isActivate)
                return;
            triggers.Enqueue(TriggerInfo.MakeStatInfo(lastStatType, modifier));
        }

        public Queue<TriggerInfo> RecordEnd()
        {
            var result = triggers;
            
            isActivate = false;
            triggers = new Queue<TriggerInfo>();
            return result;
        }
    }
}