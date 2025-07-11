using UnityEngine;
using BattleSystem;
using CharacterSystem;

public class AnimationController : MonoBehaviour
{
    private Pawn owner;
    public void Awake()
    {
        owner = gameObject.GetComponentInParent<Pawn>();
    }

    public void DestroyOnDeath()
    {
        var rb = owner.GetComponent<Rigidbody2D>();
        var Collider = owner.GetComponent<Collider2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Static; 
        if (Collider != null) Collider.enabled = false;

        CharacterFactory.Instance.Deactivate(owner);
    }
}