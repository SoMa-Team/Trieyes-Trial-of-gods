namespace Card
{
    public static class CardFactory
    {
        public static Card CreateCard(int cardId)
        {
            // 카드 데이터 하드코딩 예시
            var card = new Card
            {
                cardId = cardId,
                rarity = Rarity.Common,
                level = 1,
                property = Property.Fire,
                stat = new CardStat { bonusAttack = 5, bonusHp = 10 },
                action = null // 일단 null
            };

            // 카드 ID → CardAction ID 매핑
            int actionId = GetActionIdByCardId(cardId);
            if (actionId > 0)
                card.action = CardActionFactory.CreateAction(actionId);

            return card;
        }

        private static int GetActionIdByCardId(int cardId)
        {
            // 하드코딩 예시
            if (cardId == 1) return 1;
            if (cardId == 2) return 2;
            return 0;
        }
    }
} 