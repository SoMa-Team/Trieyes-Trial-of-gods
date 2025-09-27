using System;
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
    public class RelicStage : EventStage<RelicStage>
    {
        [SerializeField] private RelicSlotView relicSlotViewPrefab;
        [SerializeField] private RectTransform RelicListView;
        [SerializeField] private Button nextStageButton;
        
        private RelicSlotView _selectedRelicSlotView;
        private List<RelicSlotView> RelicSlotViews = new List<RelicSlotView>();

        private void Start()
        {
            nextStageButton.onClick.AddListener(NextStage);
        }

        protected override void OnActivated()
        {
            _selectedRelicSlotView = null;
            nextStageButton.interactable = false;

            for (int i = RelicListView.childCount - 1; i >= 0; i--)
            {
                Destroy(RelicListView.GetChild(i).gameObject);
            }
            
            SetRelicViews();
        }

        protected override void OnDeactivated()
        {
            _selectedRelicSlotView = null;
            foreach (var relicSlotView in RelicSlotViews)
            {
                relicSlotView.Deactivate();
                Destroy(relicSlotView.gameObject);
            }
            
            RelicSlotViews.Clear();
        }

        public override void NextStage()
        {
            mainCharacter.AddRelic(_selectedRelicSlotView.Relic);
            base.NextStage();
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
