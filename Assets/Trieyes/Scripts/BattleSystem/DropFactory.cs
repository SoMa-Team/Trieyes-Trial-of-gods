using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using BattleSystem;
using UnityEngine;

namespace BattleSystem
{
    public class DropFactory: MonoBehaviour
    {
        public static DropFactory Instance { get; private set; }
        private void Awake()
        {
            if (Instance is not null)
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

        [SerializeField] private Gold goldPrefab;

        public Gold CreateGold(Vector3 position, int goldAmount)
        {
            var goldDrop = popGold() ?? Instantiate(goldPrefab);
            goldDrop.transform.position = position;
            goldDrop.transform.SetParent(BattleStage.now.View.transform);
            Activate(goldDrop, goldAmount);
            return goldDrop;
        }

        private void Activate(Gold gold, int goldAmount)
        {
            gold.Activate(goldAmount);
            gold.gameObject.SetActive(true);
        }

        public void Deactivate(Gold gold)
        {
            gold.gameObject.SetActive(false);
            gold.Deactivate();
            pushGold(gold);
        }

        private Queue<Gold> pool = new ();
        private void pushGold(Gold gold)
        {
            pool.Enqueue(gold);
        }

        [CanBeNull]
        private Gold popGold()
        {
            if (pool.Count <= 0)
                return null;
            return pool.Dequeue();
        }
    }
}