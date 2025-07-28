using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace BattleSystem
{
    public class GoldDropFactory: MonoBehaviour
    {
        public static GoldDropFactory Instance { get; private set; }
        private void Awake()
        {
            if (Instance is not null)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        [SerializeField] private GoldDrop goldPrefab;

        public GoldDrop Create(Vector3 position, int goldAmount)
        {
            var goldDrop = popGoldDrop() ?? Instantiate(goldPrefab);
            goldDrop.transform.position = position;
            goldDrop.transform.SetParent(BattleStage.now.View.transform);
            Activate(goldDrop, goldAmount);
            return goldDrop;
        }

        private void Activate(GoldDrop goldDrop, int goldAmount)
        {
            goldDrop.Activate(goldAmount);
            goldDrop.gameObject.SetActive(true);
        }

        public void Deactivate(GoldDrop goldDrop)
        {
            goldDrop.gameObject.SetActive(false);
            goldDrop.Deactivate();
            pushGoldDrop(goldDrop);
        }

        private Queue<GoldDrop> pool = new ();
        private void pushGoldDrop(GoldDrop goldDrop)
        {
            pool.Enqueue(goldDrop);
        }

        [CanBeNull]
        private GoldDrop popGoldDrop()
        {
            if (pool.Count <= 0)
                return null;
            return pool.Dequeue();
        }
    }
}