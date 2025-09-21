using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using GamePlayer;
using OutGame;
using AttackSystem;
using Random = UnityEngine.Random;

namespace NodeStage
{
    public class SkillRewardStage : EventStage<SkillRewardStage>
    {
        [System.Serializable]
        public class IDAttackDataPair
        {
            public int id;
            public AttackData attackData;
        }

        [Header("스킬 보상 프리팹/컨테이너")]
        [SerializeField] private GameObject skillRewardPrefab;
        [SerializeField] public List<IDAttackDataPair> skillAttackData;

        private AttackData selectedSkillAttackData;
        [SerializeField] private Transform rewardContainer;

        [Header("컨트롤 UI")]
        [SerializeField] private Button nextButton;

        [Header("보상 설정값")]
        [SerializeField] private int skillChoiceCount = 3;

        private readonly List<AttackSystem.SkillView> skillSlots = new();
        private AttackSystem.SkillView selectedSkill;
        private JsonToAchivement jsonToAchivement;

        protected override void OnActivated()
        {
            if (nextButton) nextButton.interactable = false;
            jsonToAchivement = Player.Instance.jsonToAchivement;
            SetupSkillRewards();
        }

        protected override void OnDeactivated()
        {
            ClearSkillRewards();
        }

        private void SetupSkillRewards()
        {
            ClearSkillRewards();

            // 사용 가능한 스킬들 가져오기
            var availableSkills = GetAvailableSkills();
            
            // 선택할 수 있는 스킬 개수만큼 랜덤 선택
            var selectedSkills = SelectRandomSkills(availableSkills, skillChoiceCount);

            foreach (var skill in selectedSkills)
            {
                var skillObj = Instantiate(skillRewardPrefab, rewardContainer);
                var skillView = skillObj.GetComponent<AttackSystem.SkillView>();
                if (skillView != null)
                {
                    SetupSkillRewardView(skillView, skill);
                    skillSlots.Add(skillView);
                }
                else
                {
                    Debug.LogError("SkillView 컴포넌트를 찾을 수 없습니다!");
                    Destroy(skillObj);
                }
            }
        }

        private List<IAchievementObject> GetAvailableSkills()
        {
            var allSkills = jsonToAchivement.GetUnlockedAchievementByIdByType(JsonToAchivement.SKILL_TYPE, Player.Instance.mainCharacterId);
            var availableSkills = new List<IAchievementObject>();

            foreach (var skill in allSkills)
            {
                // 잠금 해제된 스킬들만 선택 가능
                if (skill.IsUnlocked)
                {
                    availableSkills.Add(skill);
                }
            }

            return availableSkills;
        }

        private List<IAchievementObject> SelectRandomSkills(List<IAchievementObject> availableSkills, int count)
        {
            var selectedSkills = new List<IAchievementObject>();
            var tempList = new List<IAchievementObject>(availableSkills);

            for (int i = 0; i < count && Math.Min(count, tempList.Count) > 0; i++)
            {
                int randomIndex = Random.Range(0, tempList.Count);
                selectedSkills.Add(tempList[randomIndex]);
                tempList.RemoveAt(randomIndex);
            }

            return selectedSkills;
        }

        private void SetupSkillRewardView(AttackSystem.SkillView skillView, IAchievementObject skill)
        {
            // 스킬 데이터 설정
            skillView.SetSkillData(skill);
            
            // AttackData 정보 설정 (있다면)
            SetAttackDataInfo(skillView, skill);
            
            // 클릭 이벤트 설정
            skillView.SetOnClicked(OnSkillClicked);
            
            // 상호작용 가능하도록 설정
            skillView.SetCanInteract(true);
            
            // 선택 상태 초기화
            skillView.SetSelected(false);
        }
        
        /// <summary>
        /// SkillView에 AttackData 정보를 설정합니다.
        /// </summary>
        private void SetAttackDataInfo(AttackSystem.SkillView skillView, IAchievementObject skill)
        {
            if (skill is SkillAchievement skillAchievement)
            {
                int skillId = skillAchievement.skillId;
                var skillAttackPair = skillAttackData.Find(pair => pair.id == skillId);
                
                if (skillAttackPair != null)
                {
                    // AttackData 정보를 SkillView에 전달
                    skillView.SetAttackDataInfo(skillAttackPair.attackData);
                }
            }
        }

        private void OnSkillClicked(AttackSystem.SkillView clickedSkillView)
        {
            if (selectedSkill == clickedSkillView)
            {
                // 같은 스킬을 다시 클릭하면 선택 해제
                clickedSkillView.SetSelected(false);
                selectedSkill = null;
            }
            else
            {
                // 다른 스킬 선택
                if (selectedSkill != null)
                {
                    selectedSkill.SetSelected(false);
                }
                
                selectedSkill = clickedSkillView;
                selectedSkill.SetSelected(true);
                selectedSkillAttackData = selectedSkill.AttackData;
            }

            // 다음 버튼 활성화 상태 업데이트
            if (nextButton) nextButton.interactable = (selectedSkill != null);
        }


        private void ClearSkillRewards()
        {
            foreach (var skillView in skillSlots)
            {
                if (skillView) Destroy(skillView.gameObject);
            }
            skillSlots.Clear();
            selectedSkill = null;
            if (nextButton) nextButton.interactable = false;
        }

        public override void NextStage()
        {
            if (selectedSkill != null)
            {
                // 선택된 스킬을 캐릭터에게 적용
                ApplySelectedSkill();
            }

            base.NextStage(); // ✅ 공통 전환
        }

        private void ApplySelectedSkill()
        {
            if (selectedSkill == null || selectedSkillAttackData == null) return;

            // selectedSkillAttackData를 직접 사용하여 캐릭터에 적용
            if (mainCharacter.skill1Attack == null)
            {
                mainCharacter.skill1Attack = selectedSkillAttackData;
            }
            else if (mainCharacter.skill2Attack == null)
            {
                // skill 2번이 비어 있으면 스킬 2번에 적용
                mainCharacter.skill2Attack = selectedSkillAttackData;
            }
            else
            {
                // 둘 다 차 있으면 스킬 1번을 교체
                mainCharacter.skill1Attack = selectedSkillAttackData;
            }
        }
    }
}
