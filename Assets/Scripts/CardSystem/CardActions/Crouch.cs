using Stats;
using System.Collections.Generic;
using CharacterSystem;
using CardSystem;
using UnityEngine;

namespace CardActions
{
    [CreateAssetMenu(menuName = "CardActions/Crouch")]
    public class Crouch : CardAction
    {
        [Header("카드 고유 설정")]
        [Tooltip("이 카드가 OnBattleSceneChange에서 올려주는 방어력 수치")]
        public int defensePowerIncrease;

        public override void OnEvent(Pawn owner, Deck deck, Utils.EventType eventType, object param)
        {
            if (owner == null || deck == null)
            {
                Debug.LogWarning("owner 또는 deck이 정의되지 않았습니다.");
                return;
            }

            if (eventType == Utils.EventType.OnBattleSceneChange)
            {
                var modifier = new StatModifier(defensePowerIncrease, BuffOperationType.Additive);
                owner.statSheet[StatType.Defense].AddBuff(modifier);
                Debug.Log($"<color=yellow>[Crouch] DEF +{defensePowerIncrease}. New Value: {owner.statSheet[StatType.Defense].Value}</color>");
            }
        }
    }
}