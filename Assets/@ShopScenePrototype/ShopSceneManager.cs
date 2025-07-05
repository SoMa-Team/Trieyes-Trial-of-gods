using UnityEngine;
using CardViews;
using UnityEngine.UI;
using CardSystem;
using DeckViews;
using CharacterSystem;
using Stats;
using TMPro;

/// <summary>
/// 상점 씬의 핵심 관리 클래스입니다.
/// 카드 3장을 뽑아 보여주고, 구매/리롤/전투진입 등
/// 상점에서 발생하는 모든 상호작용을 총괄합니다.
/// </summary>
public class ShopSceneManager : MonoBehaviour
{
    // --- 필드 ---

    [Header("카드 UI")]
    /// 상점에서 첫 번째로 보여줄 카드의 CardView 컴포넌트
    public CardView cardView1;
    /// 상점에서 두 번째로 보여줄 카드의 CardView 컴포넌트
    public CardView cardView2;
    /// 상점에서 세 번째로 보여줄 카드의 CardView 컴포넌트
    public CardView cardView3;

    [Header("구매 버튼")]
    /// 첫 번째 카드를 구매할 때 누르는 버튼
    public Button BuyButton1;
    /// 두 번째 카드를 구매할 때 누르는 버튼
    public Button BuyButton2;
    /// 세 번째 카드를 구매할 때 누르는 버튼
    public Button BuyButton3;

    [Header("전투 진입용 임시 버튼")]
    /// 전투 씬으로 넘어가는 테스트용 버튼
    public Button OnBattleSceneChange;

    [Header("리롤 버튼")]
    /// 카드 3장을 새로 뽑는(리롤) 버튼
    public Button RerollButton;
    
    [Header("카드 생성 팩토리")]
    /// 실제로 카드를 생성하는 CardFactory 참조
    public CardFactory cardFactory;

    [Header("플레이어 정보")]
    /// 상점에서 조작할 메인 캐릭터 Pawn
    public Pawn mainCharacter;

    public TMP_Text attackStatValue;
    public TMP_Text defenseStatValue;
    public TMP_Text healthStatValue;
    public TMP_Text moveSpeedStatValue;

    // --- Unity 메서드 ---

    /// <summary>
    /// 씬이 시작될 때 초기화합니다.
    /// 카드 3장을 뽑아 보여주고, 버튼들의 클릭 이벤트를 연결합니다.
    /// </summary>
    private void Start()
    {
        // 첫 리롤(시작 시 3장 뽑기)
        Reroll();
        
        // 덱 존 UI를 메인 캐릭터의 덱과 연동 (UI에 덱 표시)
        DeckZoneManager.Instance.setDeck(mainCharacter.deck);

        // 각 버튼에 클릭 이벤트 리스너 연결
        RerollButton.onClick.AddListener(Reroll);
        BuyButton1.onClick.AddListener(BuyCard1);
        BuyButton2.onClick.AddListener(BuyCard2);
        BuyButton3.onClick.AddListener(BuyCard3);
        OnBattleSceneChange.onClick.AddListener(OnBattleSceneChangeTest);
        
        statRefresh();
    }

    // --- 카드 리롤/뽑기 관련 메서드 ---

    /// <summary>
    /// 상점에 3장의 새로운 카드를 무작위로 생성해 보여줍니다.
    /// 매번 카드팩(레벨)과 카드 종류를 랜덤으로 정합니다.
    /// </summary>
    private void Reroll()
    {
        // cardFactory를 통해 3장의 무작위 카드를 생성
        Card card1 = cardFactory.Create(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(0, cardFactory.cardInfos.Count));
        Card card2 = cardFactory.Create(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(0, cardFactory.cardInfos.Count));
        Card card3 = cardFactory.Create(UnityEngine.Random.Range(1, 4), UnityEngine.Random.Range(0, cardFactory.cardInfos.Count));
        
        // 각각의 CardView에 해당 카드를 세팅 (UI에 표시)
        cardView1.SetCard(card1);
        cardView2.SetCard(card2);
        cardView3.SetCard(card3);
    }

    // --- 카드 구매 관련 메서드 ---

    /// <summary>
    /// 첫 번째 카드 구매 버튼 클릭 시 호출됩니다.
    /// 해당 카드를 메인 캐릭터의 덱에 복사본으로 추가하고, 덱 UI를 갱신합니다.
    /// </summary>
    public void BuyCard1()
    {
        // CardView에서 현재 보이는 카드 객체 참조 획득
        var card = cardView1.GetCurrentCard();
        // 실제 덱에는 딥카피하여 추가 (상점 카드와 별개)
        mainCharacter.deck.AddCard(card.DeepCopy());
        // 덱 UI를 즉시 새로고침
        DeckZoneManager.Instance.RefreshDeckUI();
    }

    /// <summary>
    /// 두 번째 카드 구매 버튼 클릭 시 호출됩니다.
    /// 나머지 동작은 BuyCard1과 동일합니다.
    /// </summary>
    public void BuyCard2()
    {
        var card = cardView2.GetCurrentCard();
        mainCharacter.deck.AddCard(card.DeepCopy());
        DeckZoneManager.Instance.RefreshDeckUI();
    }

    /// <summary>
    /// 세 번째 카드 구매 버튼 클릭 시 호출됩니다.
    /// 나머지 동작은 BuyCard1과 동일합니다.
    /// </summary>
    public void BuyCard3()
    {
        var card = cardView3.GetCurrentCard();
        mainCharacter.deck.AddCard(card.DeepCopy());
        DeckZoneManager.Instance.RefreshDeckUI();
    }

    // --- 전투 전환 테스트 메서드 ---

    /// <summary>
    /// [임시] 전투 씬 전환 버튼 클릭 시 호출.
    /// mainCharacter에게 OnBattleSceneChange 이벤트를 발생시켜,
    /// 카드의 액션/스탯 초기화 등 전투 씬 진입 처리를 실행합니다.
    /// </summary>
    public void OnBattleSceneChangeTest()
    {
        mainCharacter.OnEvent(Utils.EventType.OnBattleSceneChange, null);
        statRefresh();
    }

    private void statRefresh()
    {
        attackStatValue.text = $"{mainCharacter.statSheet[StatType.AttackPower].Value}";
        defenseStatValue.text = $"{mainCharacter.statSheet[StatType.Defense].Value}";
        healthStatValue.text = $"{mainCharacter.statSheet[StatType.Health].Value}";
        moveSpeedStatValue.text = $"{mainCharacter.statSheet[StatType.MoveSpeed].Value}";
    }
}
