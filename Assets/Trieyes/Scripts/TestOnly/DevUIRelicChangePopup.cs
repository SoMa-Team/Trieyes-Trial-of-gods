using System.Collections.Generic;
using System.Linq;
using BattleSystem;
using CharacterSystem;
using Cysharp.Threading.Tasks;
using GameFramework;
using GamePlayer;
using RelicSystem;
using UISystem;
using UnityEngine;
using UnityEngine.UI;

namespace TestOnly
{
    public class DevUIRelicChangePopup: MonoBehaviour
    {
        public static DevUIRelicChangePopup Instance { private set; get; }

        [SerializeField] private RectTransform rectTransform;
        
        [SerializeField] private RectTransform relicList;
        [SerializeField] private RelicSlotView relicSlotViewPrefab;
        
        [SerializeField] private List<Button> cancelButtons;

        private void Awake()
        {
            rectTransform.anchoredPosition = Vector2.zero;
            gameObject.SetActive(false);
            
            Instance = this;
        }

        public async UniTask Create()
        {
            for (int i = relicList.childCount - 1; i >= 0; i--)
                Destroy(relicList.GetChild(i).gameObject);
            
            PrintAllRelics();
            
            transform.SetAsLastSibling();

            BattleOverlayCanvasController.Instance?.Deactivate();
            var mainCharacter = BattleStage.now.mainCharacter;
            CharacterFactory.Instance.Deactivate(mainCharacter);
            
            gameObject.SetActive(true);
            await UniTask.WhenAny(cancelButtons.Select(b => b.OnClickAsync()));
            
            CharacterFactory.Instance.Activate(mainCharacter);
            BattleOverlayCanvasController.Instance?.Activate();
            gameObject.SetActive(false);
        }

        private void PrintAllRelics()
        {
            var allRelicIDs = Player.Instance.achievement.GetAvailableRelicIDs();

            foreach (var relicID in allRelicIDs)
            {
                var relic = RelicFactory.Create(relicID);
                var relicSlotView = Instantiate(relicSlotViewPrefab, relicList);
                relicSlotView.Activate(relic);

                var selected = BattleStage.now.mainCharacter.relics.Any(relic => relic.relicID == relicID);
                relicSlotView.SetSelected(selected, false);
                relicSlotView.SetOnClickAction(() =>
                {
                    var mainCharacter = BattleStage.now.mainCharacter;
                    var selected = mainCharacter.relics.Any(relic => relic.relicID == relicSlotView.Relic.relicID);
                    
                    if (selected)
                    {
                        var selectedRelic = mainCharacter.relics.First(relic => relic.relicID == relicSlotView.Relic.relicID);
                        mainCharacter.RemoveRelic(selectedRelic);
                        relicSlotView.SetSelected(false, false);
                    }

                    else
                    {
                        mainCharacter.AddRelic(relicSlotView.Relic);
                        relicSlotView.SetSelected(true,false);
                    }
                });
            }
        }
    }
}