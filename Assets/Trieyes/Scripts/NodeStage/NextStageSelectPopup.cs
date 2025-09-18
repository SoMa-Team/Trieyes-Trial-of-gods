using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using GameFramework;
using CharacterSystem;
using CardViews;
using GamePlayer;

namespace NodeStage
{
    public class NextStageSelectPopup : MonoBehaviour
    {
        public List<StageInfoSO> allStages;

        [SerializeField] private NextStageSlot slotPrefab;
        [SerializeField] private Transform     slotContainer;
        [SerializeField] private RectTransform rectTransform;

        [SerializeField] protected DeckView deckView;
        [SerializeField] protected Button openDeckButton;

        public static NextStageSelectPopup Instance { get; private set; }

        private readonly List<NextStageSlot> spawnedSlots = new();
        private System.Random rng = new System.Random();

        [SerializeField] protected TMP_Text bossStageLeftCountText;
        [HideInInspector] private int bossStageLeftCount => Player.Instance.bossStageLeftCount;

        [Header("다음 스테이지 고르는 알고리즘")]
        /*
        -A) SpecialNodeRate를 nodeSelectCount번 테스트 한다. 
        1번이라도 통과한다면 해당 노드에 5000번대 enum 중 1개를 할당하며 끝이 난다.
	    -B) 나머지 노드에 대하여 BattleNodeRate를 nodeSelectCount번 테스트 한다. 
        통과하면 해당 노드는 전투 노드가 되고, EliteNodeRate를 테스트하여 통과하면 엘리트, 
        그렇지 못하면 일반 전투 노드가 된다.
	    -C) BattleNodeRate를 통과하지 못하면 해당 노드는 일반 이벤트 노드 중 1개가 된다.
        */
        // TODO : 연속 전투에 따른 BattleNodeRate가 자동으로 조절되는 로직 필요. 전투 안하면 전투 강요, 전투 많이 하면 전투 안하게
        [SerializeField] private int nodeSelectCount = 3;
        [SerializeField] private float SpecialNodeRate = 0.2f;

        [SerializeField] private float StartBattleNodeRate = 0.66f; // 시작 노드 다음에 전투 등장 확률
        [SerializeField] private float BattleNodeRate = 0.66f; // 노드가 전투일 확률
        [SerializeField] private float EliteNodeRate = 0.3f; // 노드가 전투일 때 엘리트일 확률

        private static readonly HashSet<StageType> StartTypes = new()
        {
            StageType.StartCard,
            StageType.StartRelic,
        };

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (bossStageLeftCountText != null)
            {
                bossStageLeftCountText.text = "보스 까지 남은 스테이지 : " + bossStageLeftCount.ToString();
            }
        }

        private StageInfoSO GetFirstInfoOfType(StageType t)
        {
            return allStages.FirstOrDefault(s => s != null && s.type == t);
        }

        private void SpawnSlots(IEnumerable<StageInfoSO> infos, Character mainCharacter)
        {
            ClearSlots();
            foreach (var info in infos)
            {
                if (info == null) continue;
                var slot = Instantiate(slotPrefab, slotContainer);
                slot.Activate(info, chosenType =>
                {
                    InGameManager.Instance.StartNextStage(chosenType, mainCharacter);
                    Deactivate();
                });
                spawnedSlots.Add(slot);
            }
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            // 버튼 리스너 해제(중복 방지)
            if (openDeckButton != null) openDeckButton.onClick.RemoveAllListeners();

            ClearSlots();
            gameObject.SetActive(false);
        }

        private void ClearSlots()
        {
            for (int i = 0; i < spawnedSlots.Count; i++)
            {
                if (spawnedSlots[i] != null)
                {
                    spawnedSlots[i].Deactivate();
                    Destroy(spawnedSlots[i].gameObject);
                }
            }
            spawnedSlots.Clear();
        }

        public void SetNextStage(StageType? current, Character mainCharacter, bool gameStart = false)
        {
            BindDeckButton(mainCharacter);
            ClearSlots();

            if (gameStart)
            {
                ShowStartChoices(mainCharacter);
            }
            else
            {
                var options = GenerateNextStageOptions(current);
                SpawnSlots(options, mainCharacter);
            }
        }

        private List<StageInfoSO> GenerateNextStageOptions(StageType? current)
        {
            var options = new List<StageInfoSO>();
            bool bisEliteAlreadySelected = false;
            var usedStages = new HashSet<StageType>();

            Debug.Log("GenerateNextStageOptions : cnt : " + bossStageLeftCount);

            // 보스 스테이지 조건: bossStageLeftCount가 0이면 모든 노드를 보스로 설정
            if (bossStageLeftCount <= 0)
            {
                for (int i = 0; i < nodeSelectCount; i++)
                {
                    var bossStage = GetFirstInfoOfType(StageType.Boss);
                    if (bossStage != null)
                        options.Add(bossStage);
                }
                return options;
            }

            // 일반 스테이지 선택 알고리즘
            for (int i = 0; i < nodeSelectCount; i++)
            {
                StageInfoSO selectedStage = null;

                // A) SpecialNodeRate 테스트 (5000번대 노드)
                if (rng.NextDouble() < SpecialNodeRate)
                {
                    selectedStage = GetRandomSpecialEventStage();
                }
                else
                {
                    // B) BattleNodeRate 테스트 (시작 노드 다음에는 StartBattleNodeRate 사용)
                    float battleRate = IsStartStage(current) ? StartBattleNodeRate : BattleNodeRate;
                    
                    if (rng.NextDouble() < battleRate)
                    {
                        // EliteNodeRate 테스트
                        if (rng.NextDouble() < EliteNodeRate && !bisEliteAlreadySelected)
                        {
                            selectedStage = GetFirstInfoOfType(StageType.Elite);
                            bisEliteAlreadySelected = true;
                        }
                        else
                        {
                            selectedStage = GetFirstInfoOfType(StageType.Battle);
                        }
                    }
                    else
                    {
                        // C) 일반 이벤트 노드
                        selectedStage = GetRandomNormalEventStage();
                    }
                }

                if (selectedStage != null && !usedStages.Contains(selectedStage.type))
                {
                    options.Add(selectedStage);
                    usedStages.Add(selectedStage.type);
                }
                else if (selectedStage != null && usedStages.Contains(selectedStage.type))
                {
                    // 중복된 경우 대체 노드 선택
                    var alternativeStage = GetAlternativeStage(usedStages, current);
                    if (alternativeStage != null)
                    {
                        options.Add(alternativeStage);
                        usedStages.Add(alternativeStage.type);
                    }
                }
            }

            return options;
        }

        private StageInfoSO GetRandomSpecialEventStage()
        {
            var specialStages = allStages
                .Where(s => s != null && IsSpecialEventStage(s.type))
                .ToList();

            if (specialStages.Count == 0) return null;

            int randomIndex = rng.Next(0, specialStages.Count);
            return specialStages[randomIndex];
        }

        private StageInfoSO GetRandomNormalEventStage()
        {
            var normalStages = allStages
                .Where(s => s != null && IsNormalEventStage(s.type))
                .ToList();

            if (normalStages.Count == 0) return null;

            int randomIndex = rng.Next(0, normalStages.Count);
            return normalStages[randomIndex];
        }

        private bool IsSpecialEventStage(StageType stageType)
        {
            // 5000번대: 특별 이벤트 보상 노드
            return (int)stageType >= 5000 && (int)stageType < 6000;
        }

        private bool IsNormalEventStage(StageType stageType)
        {
            // 1000번대: 일반 이벤트 보상 노드
            return (int)stageType >= 1000 && (int)stageType < 2000;
        }

        private bool IsStartStage(StageType? stageType)
        {
            return stageType.HasValue && StartTypes.Contains(stageType.Value);
        }

        private StageInfoSO GetAlternativeStage(HashSet<StageType> usedStages, StageType? current)
        {
            // 사용되지 않은 모든 스테이지 중에서 랜덤 선택
            var availableStages = allStages
                .Where(s => s != null && !usedStages.Contains(s.type) && s.type != current && 
                           !StartTypes.Contains(s.type) && s.type != StageType.Boss && s.type != StageType.GameOver)
                .ToList();

            if (availableStages.Count == 0) return null;

            int randomIndex = rng.Next(0, availableStages.Count);
            return availableStages[randomIndex];
        }

        private void ShowStartChoices(Character mainCharacter)
        {
            var startInfos = new List<StageInfoSO>
                {
                    GetFirstInfoOfType(StageType.StartCard),
                    GetFirstInfoOfType(StageType.StartRelic),
                }
                .Where(x => x != null)
                .ToList();

            SpawnSlots(startInfos, mainCharacter);
        }

        private void BindDeckButton(Character mainCharacter)
        {
            if (openDeckButton == null) return;

            openDeckButton.onClick.RemoveAllListeners();
            
            openDeckButton.onClick.AddListener(() => OpenDeckInspectOnly(mainCharacter));
            openDeckButton.interactable = true;
        }

        protected void OpenDeckInspectOnly(Character mainCharacter)
        {
            if (deckView == null || mainCharacter == null) return;
            deckView.Activate(mainCharacter.deck, requiredCount: 0, onConfirm: null, onCancel: null);
        }
    }
}
