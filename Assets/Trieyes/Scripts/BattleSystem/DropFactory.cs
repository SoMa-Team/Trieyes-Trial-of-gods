using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using BattleSystem;
using PrimeTween;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleSystem
{
    public class DropFactory: MonoBehaviour
    {
        public static DropFactory Instance { get; private set; }
        private static int _goldObjectID;
        private void Awake()
        {

            
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        [SerializeField] private Gold goldPrefab;

        public Gold CreateGold(Vector3 position, int goldAmount, bool isSetActive = true)
        {
            var goldDrop = popGold();
            if (goldDrop is null)
            {
                goldDrop = Instantiate(goldPrefab);
                goldDrop.objectID = getObjectID();
            }
            goldDrop.transform.position = position;
            goldDrop.transform.SetParent(BattleStage.now.View.transform);
            Activate(goldDrop, goldAmount, isSetActive);
            return goldDrop;
        }

        private void Activate(Gold gold, int goldAmount, bool isSetActive)
        {
            gold.Activate(goldAmount);

            if (isSetActive)
            {
                gold.gameObject.SetActive(true);
                gold.isActive = true;
            }
            else {
                gold.isActive = false;
            }
        }

        public void Deactivate(Gold gold)
        {
            if (gold == null) return;
            gold.gameObject.SetActive(false);
            gold.Deactivate();
            BattleStage.now.RemoveGold(gold);
            pushGold(gold);
        }

        private Queue<Gold> pool = new ();
        private void pushGold(Gold gold)
        {
            pool.Enqueue(gold);
        }

        public void ClearPool()
        {
            pool.Clear();
        }

        [CanBeNull]
        private Gold popGold()
        {
            if (pool.Count <= 0)
                return null;
            return pool.Dequeue();
        }
        
        private int getObjectID()
        {
            return _goldObjectID++;
        }

        public Tween AnimationDrop(Gold gold)
        {
            float radiusBase = 2f;
            float radiusNoise = 2f;
            float duration = 1f;
            
            var radius = Random.Range(radiusBase - radiusNoise, radiusBase + radiusNoise);
            var angleRad = Random.Range(0, 360) * Mathf.Deg2Rad;
            var position = new Vector3(radius * Mathf.Cos(angleRad), radius * Mathf.Sin(angleRad), 0);

            var startPosition = gold.transform.position;
            var targetPosition = gold.transform.position + position;
            
            return Tween.Custom(0f, 1f, duration, t =>
            {
                var position = new Vector3(0, 0, 0);
                position.x = Mathf.Lerp(startPosition.x, targetPosition.x, t);
                position.y = Mathf.Lerp(startPosition.y, targetPosition.y, t);
                position.y += 5 * t * (1 - t);
                if (gold != null)
                    gold.transform.position = position;
            }).OnComplete(() =>
            {
                if (gold != null)
                    gold.isActive = true;
            });
        }
    }
}