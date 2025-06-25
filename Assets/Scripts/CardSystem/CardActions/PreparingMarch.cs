using UnityEngine;
using CharacterSystem;
using CardSystem;
using Stats;
using System.Collections.Generic;
using BattleSystem;

namespace CardActions
{
    [CreateAssetMenu(menuName = "CardActions/PreparingMarch")]
    public class PreparingMarch : CardAction
    {
        [Header("카드 고유 설정")]
        [Tooltip("이 카드가 OnBattleSceneChange에서 올려주는 공격력 수치")]
        public int attackPowerIncrease;

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                Debug.Log($"PreparingMarch.OnEvent: OnBattleSceneChange");
                var modifier = new StatModifier(attackPowerIncrease, BuffOperationType.Additive);
                Debug.Log("modifier Created");
                owner.statSheet[StatType.AttackPower].AddBuff(modifier);
                Debug.Log($"<color=yellow>[PreparingMarch] ATK +{attackPowerIncrease}. New Value: {owner.statSheet[StatType.AttackPower].Value}</color>");
            }
        }
    }
} 