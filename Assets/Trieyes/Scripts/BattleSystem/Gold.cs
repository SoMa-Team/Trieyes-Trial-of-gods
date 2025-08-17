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
        private bool isActive = false;
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
                // TODO: 추후에 Shop이 같은 Scene이 된다면, 오류 출력하도록 조정 필요.
                Tween.Custom(0, 1, 1.0f, t =>
                {
                    if (BattleStage.now == null)
                        return;
                    
                    var character = BattleStage.now.mainCharacter;
                    transform.position = Vector3.Lerp(position, character.transform.position, t * t);  
                }).OnComplete(() =>
                {
                    character.ChangeGold(goldAmount);
                    DropFactory.Instance.Deactivate(this);
                }, warnIfTargetDestroyed: false);
            }
        }

        private float GetGoldCollisionDistance(Pawn pawn)
        {
            var magnet = pawn.statSheet[StatType.ItemMagnet].Value;
            var resultDistance = 0.2f + MathF.Log(magnet + 1);
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
