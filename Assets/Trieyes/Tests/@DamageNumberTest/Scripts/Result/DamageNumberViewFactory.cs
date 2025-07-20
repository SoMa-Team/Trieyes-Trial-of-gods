using System;
using System.Collections.Generic;
using AttackSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.WSA;

public class DamageNumberViewFactory : MonoBehaviour
{
    [SerializeField] public RectTransform targetRectTransform;
    
    // ===== 싱글톤 =====
    public static DamageNumberViewFactory Instance { get; private set; } 
    private void Awake()
    {
        if (Instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        
        view.Activate();
        
        view.gameObject.SetActive(true);
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
}
