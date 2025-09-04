using UnityEngine;
using TMPro;
using GamePlayer;
using BattleSystem;
using CharacterSystem;
using UnityEngine.UI;
using GameFramework;
using CardSystem;
using CardViews;

namespace OutGame{
    public class CardSelectListView : MonoBehaviour
    {
        public const int CARD_COUNT = 3;
        public Vector2 CARD_SCALE = new Vector2(0.75f, 0.75f);
        public CardSelectView cardSelectView;

        public GameObject cardSelectViewPrefab;
        public Card selectedCard;
        
        // 초기화 메서드
        public void Awake()
        {
            if (cardSelectViewPrefab != null)
            {
                for (int i = 0; i < CARD_COUNT; i++)
                {
                    var obj = Instantiate(cardSelectViewPrefab, transform);
                    obj.transform.localScale = new Vector3(CARD_SCALE.x, CARD_SCALE.y, 1);
                    cardSelectView = obj.GetComponent<CardSelectView>();
                    cardSelectView.SetCardSelectListView(this);
                    cardSelectView.SetCardView(obj.GetComponent<CardView>());

                    obj.SetActive(true);
                }
            }
        }

        public void ToRelicSelectPanel()
        {
            StartSceneManager.Instance.selectedCard = selectedCard;
            StartSceneManager.Instance.ToRelicSelectPanel();
        }
    }
}