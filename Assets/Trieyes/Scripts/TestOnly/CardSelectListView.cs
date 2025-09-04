using UnityEngine;
using CardSystem;
using CardViews;

namespace OutGame{
    public class CardSelectListView : ListView
    {
        public const int CARD_COUNT = 3;
        public Vector2 CARD_SCALE = new Vector2(0.75f, 0.75f);
        public CardSelectView cardSelectView;

        public GameObject cardSelectViewPrefab;
        public Card selectedCard;

        public void ToRelicSelectPanel()
        {
            StartSceneManager.Instance.selectedCard = selectedCard;
            StartSceneManager.Instance.ToRelicSelectPanel();
        }
        
        /// <summary>
        /// 카드 선택 패널이 활성화될 때 호출됩니다.
        /// 새로운 랜덤 카드들을 생성합니다.
        /// </summary>
        public override void Activate()
        {
            // 새로운 카드들 생성
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

            base.Activate();
        }
        
        /// <summary>
        /// 카드 선택 패널이 비활성화될 때 호출됩니다.
        /// 선택된 카드를 초기화합니다.
        /// </summary>
        public override void Deactivate()
        {
            // 자식 오브젝트들을 모두 제거
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            selectedCard = null;

            base.Deactivate();
        }
    }
}