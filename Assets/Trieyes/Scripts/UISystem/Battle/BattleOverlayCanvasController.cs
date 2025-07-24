using System;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
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

        [Header("========== Skills ==========")]
        [SerializeField] private Image BasicAttackIconView;
        [SerializeField] private Image Skill1IconView;
        [SerializeField] private Image Skill2IconView;

        [SerializeField] private Image BasicAttackCoolDown;
        [SerializeField] private Image Skill1CoolDown;
        [SerializeField] private Image Skill2CoolDown;
        
        public void Activate()
        {
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
    }
}
