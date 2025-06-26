using UnityEngine;
using UnityEngine.UI;
using CardSystem;
using CharacterSystem;
using Stats;
using BattleSystem;
using CharacterSystem;
using Utils;
public class TestCardSpawnerWithButton : MonoBehaviour
{
    [Header("참조")]
    public Pawn pawn;        // 인스펙터에서 할당
    public Button testButton; // 인스펙터에서 UI Button 할당

    [Header("테스트 카드 설정")]
    public int testCardActionID = 0;
    public int cardLevel = 1;

    private void Start()
    {
        if (testButton != null)
        {
            testButton.onClick.AddListener(SpawnAndTestCard);
            Debug.Log("<color=orange>[TEST] 버튼에 SpawnAndTestCard() 이벤트 연결 완료</color>");
        }
        else
        {
            Debug.LogWarning("<color=red>[TEST] testButton이 연결되어 있지 않습니다!</color>");
        }

        pawn.Activate();
        Difficulty difficulty = Difficulty.GetByStageRound(0);
        BattleStage battleStage = BattleStageFactory.Instance.Create(pawn, difficulty);
    }

    private void SpawnAndTestCard()
    {
        Debug.Log("<color=yellow>[TEST] === 카드 생성/덱 등록 테스트 시작 ===</color>");

        // 1. 카드 생성
        Debug.Log($"[TEST] 1. CardFactory로 카드 생성 시도 (ActionID={testCardActionID}, Level={cardLevel})");
        Card newCard = CardFactory.Instance.Create(cardLevel, testCardActionID);
        if (newCard == null)
        {
            Debug.LogError("[TEST] 카드 생성 실패! CardFactory.Instance 또는 CardActionFactory.Instance가 null일 수 있음.");
            return;
        }
        Debug.Log($"[TEST] 1. 생성된 카드: cardId={newCard.cardId}, cardActionSO={newCard.cardActionSO?.cardName}");

        // 2. 덱에 카드 추가
        Debug.Log("[TEST] 2. Deck에 카드 추가");
        pawn.deck.AddCard(newCard);
        Debug.Log($"[TEST] 2. Deck 카드 개수: {pawn.deck.Cards.Count}");

        // 3. 덱 초기화
        Debug.Log("[TEST] 3. Deck.Initialize() 실행 (owner/persistent 재설정)");
        pawn.deck.Initialize(pawn, isPersistent: true);
        Debug.Log("[TEST] 3. Deck 초기화 완료");

        // 4. 전투 시작 이벤트 발동
        Debug.Log("[TEST] 4. Deck.OnEvent(OnBattleStart) 호출 (카드 효과 발동)");
        pawn.deck.OnEvent(Utils.EventType.OnBattleStart, null);
        Debug.Log("Test4 종료");

        // 5. Pawn 스탯 결과 확인
        int atk = pawn.statSheet[StatType.AttackPower].Value;
        Debug.Log($"<color=lime>[TEST] 5. 전투 시작 후 Pawn ATK: {atk}</color>");

        Debug.Log("<color=yellow>[TEST] === 카드 테스트 시퀀스 종료 ===</color>");
    }
}
