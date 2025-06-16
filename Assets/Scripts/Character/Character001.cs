using UnityEngine;
using Core;
using System.Linq;
using CharacterSystem;
using AttackSystem;
using Utils;

namespace CharacterSystem
{
    public class Character001 : Pawn
    {
        // Character001 고유의 필드 또는 속성
        public int experience = 0;
        public int gold = 0;

        protected override void Awake()
        {
            base.Awake();
            initBaseStat();
            Debug.Log("Character001 Awake.");
        }

        protected override void initBaseStat()
        {
            base.initBaseStat();

            if (!statInfos.Any(s => s.type == StatType.AttackPower))
            {
                statInfos.Add(new StatInfo(StatType.AttackPower, 20));
            }
            if (!statInfos.Any(s => s.type == StatType.Defense))
            {
                statInfos.Add(new StatInfo(StatType.Defense, 10));
            }
        }

        public override void OnEvent(Core.EventType eventType, object param)
        {
            // 먼저 부모 클래스의 이벤트 처리
            base.OnEvent(eventType, param);

            // Character001 고유의 이벤트 처리
            switch (eventType)
            {
                case Core.EventType.OnLevelUp:
                    // 레벨업 시 스탯 증가
                    IncreaseStatValue(StatType.AttackPower, 5);
                    IncreaseStatValue(StatType.Defense, 3);
                    break;
                case Core.EventType.OnDeath:
                    if ((Object)param == this) // Object 타입으로 캐스팅하여 비교
                    {
                        Debug.Log($"<color=yellow>{gameObject.name} (Character001) is performing its unique death action: Game Over!</color>");
                        // 게임 오버 처리
                    }
                    else if (param is Pawn deadPawn) // 다른 Pawn의 사망 이벤트인 경우
                    {
                        Debug.Log($"Character001: {deadPawn.gameObject.name}의 사망을 감지했습니다.");
                        // 적 사망 시 특정 버프를 얻거나, 퀘스트 진행도 업데이트 등의 로직 구현
                    }
                    break;
                // 필요한 다른 이벤트 케이스들을 여기에 추가
            }
        }
    }
} 