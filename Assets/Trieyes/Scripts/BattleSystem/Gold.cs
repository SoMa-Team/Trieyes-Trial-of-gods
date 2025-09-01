using System;
using BattleSystem;
using CharacterSystem;
using PrimeTween;
using Stats;
using UnityEngine;

namespace BattleSystem
{
    public class Gold : MonoBehaviour
    {
        public bool isActive = false;
        [SerializeField] private int goldAmount;
        [HideInInspector] public int objectID;

        private void Update()
        {
            if (!isActive)
                return;

            var character = BattleStage.now.mainCharacter;
            var position = transform.position;
            var characterPosition = character.transform.position;
            var distance = Vector3.Distance(characterPosition, position);

            if (distance < GetGoldCollisionDistance(BattleStage.now.mainCharacter))
            {
                isActive = false;
                Tween.Custom(0, 1, 0.4f, t =>
                {
                    if (BattleStage.now == null)
                        return;
                    
                    var character = BattleStage.now.mainCharacter;
                    transform.position = Vector3.Lerp(position, character.transform.position, t * t);  
                }).OnComplete(() =>
                {
                    character.ChangeGold(goldAmount);
                    DropFactory.Instance.Deactivate(this);
                });
            }
        }

        private float GetGoldCollisionDistance(Pawn pawn)
        {
            var magnet = pawn.statSheet[StatType.ItemMagnet].Value;
            var resultDistance = (1 + magnet) * 2f;
            return resultDistance;
        }

        public void Activate(int goldAmount)
        {
            isActive = true;
            this.goldAmount = goldAmount;
        }

        public void Deactivate()
        {
            goldAmount = 0;
        }
    }
}
