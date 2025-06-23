using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardActions;

namespace CardSystem
{
    public class CardFactory : MonoBehaviour
    {
        public static CardFactory Instance { private set; get; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public Card Create(int level, int CardActionID)
        {
            Card card = new Card();
            Activate(card, level, CardActionID);
            return card;
        }

        public void Activate(Card card, int level, int CardActionID)
        {
            card.Activate(level, CardActionID);
        }

        public void Deactivate(Card card)
        {
            card.Deactivate();
        }
    }
} 