using System.Collections.Generic;
using CharacterSystem;
using GameFramework;
using GamePlayer;
using RelicSystem;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace NodeStage
{
    public class StartRelicStage : MonoBehaviour, NodeStage
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private RelicSlotView relicSlotViewPrefab;
        [SerializeField] private RectTransform RelicListView;
        [SerializeField] private Button nextStageButton;
        
        private Character mainCharacter;
        private RelicSlotView _selectedRelicSlotView;
        private List<RelicSlotView> RelicSlotViews = new List<RelicSlotView>();
        
        public static StartRelicStage Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            gameObject.SetActive(false);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        public void Activate(Character mainCharacter)
        {
            this.mainCharacter = mainCharacter;
            _selectedRelicSlotView = null;
            nextStageButton.interactable = false;
            
            gameObject.SetActive(true);
            SetRelicViews();
        }

        private void DeActivate()
        {
            _selectedRelicSlotView = null;
            foreach (var relicSlotView in RelicSlotViews)
            {
                relicSlotView.Deactivate();
                Destroy(relicSlotView.gameObject);
            }
            
            RelicSlotViews.Clear();
            gameObject.SetActive(false);
        }

        public void NextStage()
        {
            mainCharacter.AddRelic(_selectedRelicSlotView.Relic);
            
            DeActivate();
            NextStageSelectPopup.Instance.SetNextStage(StageType.StartRelic, mainCharacter);
        }
        
        private void SetRelicViews()
        {
            const int relicCount = 3;

            var allAvailableRelicIDs = Player.Instance.achievement.GetAvailableRelicIDs();
            allAvailableRelicIDs.Shuffle();
            
            for (int i = 0; i < relicCount; i++)
            {
                var relic = RelicFactory.Create(allAvailableRelicIDs[i]);
                var relicView = Instantiate(relicSlotViewPrefab, RelicListView);
                relicView.Activate(relic);
                relicView.SetOnClickAction(() =>
                {
                    _selectedRelicSlotView?.SetSelected(false);
                    
                    _selectedRelicSlotView = relicView;
                    _selectedRelicSlotView.SetSelected(true);
                    
                    nextStageButton.interactable = true;
                });
                
                RelicSlotViews.Add(relicView);
            }
        }
    }
}