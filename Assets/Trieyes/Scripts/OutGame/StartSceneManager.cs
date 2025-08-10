using System;
using UnityEngine;
using UnityEngine.UI;
using CharacterSystem;
using GameFramework;
using System.Threading.Tasks;
using RelicSystem;
using AttackSystem;
using GamePlayer;

namespace OutGame{
    public class StartSceneManager : MonoBehaviour
    {

        public Player player;
        
        public GameObject CharacterSelectPanel;
        public GameObject SkillSelectPanel;
        public GameObject RelicSelectPanel;

        async void Start()
        {
            CharacterSelectPanel.SetActive(false);
            SkillSelectPanel.SetActive(false);
            RelicSelectPanel.SetActive(false);
            
            await _Start();
        }

        async Task _Start()
        {
            Debug.Log("Initialize Async");
            await RelicDataBase.InitializeAsync();
        }

        public void GameStart()
        {
            SceneChangeManager.Instance.StartBattleSceneTest();
        }
        
        public void ToCharacterSelectPanel()
        {
            CharacterSelectPanel.SetActive(true);
        }
        
        public void CloseCharacterSelectPanel()
        {
            CharacterSelectPanel.SetActive(false);
        }
        
        public void ToSkillSelectPanel()
        {
            SkillSelectPanel.SetActive(true);
        }
        
        public void CloseSkillSelectPanel()
        {
            SkillSelectPanel.SetActive(false);
        }

        public void ToRelicSelect()
        {
            RelicSelectPanel.SetActive(true);
        }

        public void CloseRelicSelect()
        {
            RelicSelectPanel.SetActive(false);
        }
    }
}