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

        void Start()
        {
            Debug.Log("StartSceneManager");
            CharacterSelectPanel?.SetActive(false); //TODO: 패널 추가시 ? 해제
            SkillSelectPanel?.SetActive(false); //TODO: 패널 추가시 ? 해제
            RelicSelectPanel.SetActive(false);
            
            _Start();
        }

        async Task _Start()
        {
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