using System;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace UISystem
{
    public class BattleOverlayCanvasController : MonoBehaviour
    {
        public static BattleOverlayCanvasController Instance { get; private set; }

        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        [Header("========== Joysticks ==========")]
        [SerializeField] public Joystick joystick;
        
        [Header("========== StageInfos ==========")]
        [SerializeField] private TextMeshProUGUI StageNumberText;
        [SerializeField] private TextMeshProUGUI StageRemainTimeText;
        [SerializeField] private Slider StageRemainTimeSlider;

        [Header("========== Skills ==========")]
        [SerializeField] private Image BasicAttackIconView;
        [SerializeField] private Image Skill1IconView;
        [SerializeField] private Image Skill2IconView;

        [SerializeField] private Image BasicAttackCoolDown;
        [SerializeField] private Image Skill1CoolDown;
        [SerializeField] private Image Skill2CoolDown;

        [Header("========== Auto Attack ==========")]
        [SerializeField] private GameObject[] AutoBasicAttackOnViews;
        [SerializeField] private GameObject[] AutoBasicAttackOffViews;
        
        public void Activate()
        {
            Debug.LogWarning($"Activate: {BattleStage.now.mainCharacter}, \n Attack : {BattleStage.now.mainCharacter.basicAttack}, \n Skill1 : {BattleStage.now.mainCharacter.skill1Attack}, \n Skill2 : {BattleStage.now.mainCharacter.skill2Attack}");
            var basicAttackAddress = buildAttackIconAddress(BattleStage.now.mainCharacter.basicAttack.attackIcon);
            Addressables.LoadAssetAsync<Sprite>(basicAttackAddress).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load Basic Attack Icon {basicAttackAddress}");
                SetBasicAttackIcon(handle.Result);
            };
            
            var skill1Address = buildAttackIconAddress(BattleStage.now.mainCharacter.skill1Attack.attackIcon);
            Addressables.LoadAssetAsync<Sprite>(skill1Address).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load Skill1 Icon {skill1Address}");
                SetSkill1AttackIcon(handle.Result);
            };
            
            var skill2Address = buildAttackIconAddress(BattleStage.now.mainCharacter.skill2Attack.attackIcon);
            Addressables.LoadAssetAsync<Sprite>(skill2Address).Completed += (AsyncOperationHandle<Sprite> handle) =>
            {
                if (handle.Status != AsyncOperationStatus.Succeeded)
                    throw new Exception($"Failed to load Skill2 Icon {skill2Address}");
                SetSkill2AttackIcon(handle.Result);
            };

            SetStageNumber();
            InitAutoAttackToggle();
            testPanel.gameObject.SetActive(false);
            gameObject.SetActive(true);
        }

        private void InitAutoAttackToggle()
        {
            var isAuto = BattleStage.now.mainCharacter.isAutoAttack;
            SetAutoAttack(isAuto);
        }

        private void SetAutoAttack(bool isAuto)
        {
            foreach (var view in AutoBasicAttackOnViews)
            {
                view.SetActive(isAuto);
            }
            
            foreach (var view in AutoBasicAttackOffViews)
            {
                view.SetActive(!isAuto);
            }
        }

        private void SetStageNumber()
        {
            StageNumberText.text = $"{BattleStage.now.difficulty.stageNumber.ToString()}-3";
        }

        private string buildAttackIconAddress(string name)
        {
            return $"Assets/Trieyes/Addressable/Icons/Skills/{name}";
        }

        private void Update()
        {
            if (BattleStage.now is null)
                return;
            
            var character = BattleStage.now.mainCharacter;
            SetBasicAttackCoolDown(character.BasicAttackCoolDownRate);
            SetSkill1CoolDown(character.Skill1CoolDownRate);
            SetSkill2CoolDown(character.Skill2CoolDownRate);

            float elapsedTime = BattleStage.now.elapsedTime;
            float totalTime = BattleStage.now.difficulty.battleLength;
            SetRemainTime(elapsedTime, totalTime);
        }

        private void SetRemainTime(float elapsedTime, float totalTime)
        {
            StageRemainTimeText.text = $"{Mathf.Floor(totalTime - elapsedTime)}s";
            StageRemainTimeSlider.value = elapsedTime / totalTime;
        }

        private void SetBasicAttackIcon(Sprite sprite)
        {
            BasicAttackIconView.sprite = sprite;
        }

        private void SetSkill1AttackIcon(Sprite sprite)
        {
            Skill1IconView.sprite = sprite;
        }

        private void SetSkill2AttackIcon(Sprite sprite)
        {
            Skill2IconView.sprite = sprite;
        }

        private void SetBasicAttackCoolDown(float rate)
        {
            if (BattleStage.now.mainCharacter.isAutoAttack)
            {
                BasicAttackCoolDown.fillAmount = 1;
                return;
            }
            
            BasicAttackCoolDown.fillAmount = rate;
        }

        private void SetSkill1CoolDown(float rate)
        {
            Skill1CoolDown.fillAmount = rate;
        }

        private void SetSkill2CoolDown(float rate)
        {
            Skill2CoolDown.fillAmount = rate;
        }

        public void OnClickBasicAttack()
        {
            BattleStage.now.mainCharacter.ExecuteAttack(PawnAttackType.BasicAttack);
        }

        public void OnClickSkill1()
        {
            BattleStage.now.mainCharacter.ExecuteAttack(PawnAttackType.Skill1);
        }

        public void OnClickSkill2()
        {
            BattleStage.now.mainCharacter.ExecuteAttack(PawnAttackType.Skill2);
        }

        public void OnClickAutoAttack()
        {
            var character = BattleStage.now.mainCharacter;
            character.isAutoAttack = !character.isAutoAttack;
            SetAutoAttack(character.isAutoAttack);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
        
        // ===== Test Panel Buttons =====
        [Header("========== Test Panel ==========")]
        [SerializeField] RectTransform testPanel;
        public void OnClickTestPause()
        {
            var isActivate = testPanel.gameObject.activeSelf;
            testPanel.anchoredPosition = Vector2.zero;
            testPanel.gameObject.SetActive(!isActivate);
        }
        
        public void Test_OnClickSkipStage()
        {
            BattleStage.now.OnBattleClear();
        }

        public void Test_OnClickAddTime()
        {
            BattleStage.now.AddTime(9999f);
        }
    }
}
