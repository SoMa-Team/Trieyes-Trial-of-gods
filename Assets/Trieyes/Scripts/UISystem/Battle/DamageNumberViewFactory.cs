using System.Collections.Generic;
using AttackSystem;
using JetBrains.Annotations;
using UnityEngine;

namespace UISystem
{
    public class DamageNumberViewFactory : MonoBehaviour
    {
        [SerializeField] public RectTransform targetRectTransform;
    
        // ===== 싱글톤 =====
        public static DamageNumberViewFactory Instance { get; private set; } 
        private void Awake()
        {

        
            Instance = this;
        }
    
        // ===== DamageView 생성 =====
        [SerializeField] public DamageNumberView prefab;
    
        public DamageNumberView Create(AttackResult result)
        {
            var view = popDamageNumberView() ?? Instantiate(prefab);
            Activate(view, result);
            return view;
        }

        // ===== Object Pooling =====
        private Queue<DamageNumberView> pool = new ();

        public void Activate(DamageNumberView view, AttackResult result)
        {
            view.targetRectTransform = targetRectTransform;
        
            view.SetDamage(result);
            view.SetPosition(result.target.transform.position);
        
            view.gameObject.SetActive(true);
            view.Activate();
        }

        public void Deactivate(DamageNumberView view)
        {
            view.gameObject.SetActive(false);
        
            view.Deactivate();
        
            pool.Enqueue(view);
        }

        [CanBeNull]
        private DamageNumberView popDamageNumberView()
        {
            if (pool.Count <= 0)
                return null;
            return pool.Dequeue();
        }

        public void OnBattleEnded()
        {
            //Instance = null;
        }
    }
}
