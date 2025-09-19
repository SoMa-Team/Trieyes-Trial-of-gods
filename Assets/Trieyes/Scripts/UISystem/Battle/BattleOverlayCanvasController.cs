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
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        private void OnDestroy()
        {
            Instance = null;
        }
        
        [Header("========== Joysticks ==========")]
        [SerializeField] public Joystick joystick;
        
        [Header("========== StageInfos ==========")]
        [SerializeField] private TextMeshProUGUI StageNumberText;
        [SerializeField] private TextMeshProUGUI StageRemainTimeText;
        [SerializeField] private TextMeshProUGUI EnemyRemainCountText;
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
            Addressables.LoadAssetAsync<Sprite>(basicAttackAddress).Completed +=
                (AsyncOperationHandle<Sprite> handle) =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                        throw new Exception($"Failed to load Basic Attack Icon {basicAttackAddress}");
                    SetBasicAttackIcon(handle.Result);
                };

            // TODO: 스킬이 없을 때도 고려
            var skill1Address = buildAttackIconAddress(BattleStage.now.mainCharacter.skill1Attack?.attackIcon);
            if (skill1Address != "")
            {
                Addressables.LoadAssetAsync<Sprite>(skill1Address).Completed += (AsyncOperationHandle<Sprite> handle) =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                        throw new Exception($"Failed to load Skill1 Icon {skill1Address}");
                    SetSkill1AttackIcon(handle.Result);
                };
            }

            var skill2Address = buildAttackIconAddress(BattleStage.now.mainCharacter.skill2Attack?.attackIcon);
            if (skill2Address != "")
            {
                Addressables.LoadAssetAsync<Sprite>(skill2Address).Completed += (AsyncOperationHandle<Sprite> handle) =>
                {
                    if (handle.Status != AsyncOperationStatus.Succeeded)
                        throw new Exception($"Failed to load Skill2 Icon {skill2Address}");
                    SetSkill2AttackIcon(handle.Result);
                };
            }
            
            SetStageNumber();
            InitAutoAttackToggle();
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
                if (view != null) view.SetActive(isAuto);
            }
            
            foreach (var view in AutoBasicAttackOffViews)
            {
                if (view != null) view.SetActive(!isAuto);
            }
        }

        private void SetStageNumber()
        {
            StageNumberText.text = $"{BattleStage.now.difficulty.stageNumber.ToString()}-3";
        }

        private string buildAttackIconAddress(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "";
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

            if (BattleStage.now is BattleBreakThrough)
            {
                StageRemainTimeText.text = $"-s";
                var breakThrough = BattleStage.now as BattleBreakThrough;
                EnemyRemainCountText.text = $"{breakThrough.GetBreakThroughCount()}";
            }
            if (BattleStage.now is BattleTimer)
            {
                EnemyRemainCountText.text = $"0";
                float elapsedTime = BattleStage.now.elapsedTime;
                float totalTime = BattleStage.now.difficulty.battleLength;
                SetRemainTime(elapsedTime, totalTime);
            }
        }

        private void SetRemainTime(float elapsedTime, float totalTime)
        {
            StageRemainTimeText.text = $"{Mathf.Floor(totalTime - elapsedTime)}s";
            StageRemainTimeSlider.value = elapsedTime / totalTime;
        }

        private void SetBasicAttackIcon(Sprite sprite)
        {
            if (BasicAttackIconView != null) BasicAttackIconView.sprite = sprite;
        }

        private void SetSkill1AttackIcon(Sprite sprite)
        {
            if (Skill1IconView != null) Skill1IconView.sprite = sprite;
        }

        private void SetSkill2AttackIcon(Sprite sprite)
        {
            if (Skill2IconView != null) Skill2IconView.sprite = sprite;
        }

        private void SetBasicAttackCoolDown(float rate)
        {
            if (BasicAttackCoolDown == null) return;
            
            if (BattleStage.now.mainCharacter.isAutoAttack)
            {
                BasicAttackCoolDown.fillAmount = 1;
                return;
            }
            
            BasicAttackCoolDown.fillAmount = rate;
        }

        private void SetSkill1CoolDown(float rate)
        {
            if (Skill1CoolDown != null) Skill1CoolDown.fillAmount = rate;
        }

        private void SetSkill2CoolDown(float rate)
        {
            if (Skill2CoolDown != null) Skill2CoolDown.fillAmount = rate;
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
    }
}
