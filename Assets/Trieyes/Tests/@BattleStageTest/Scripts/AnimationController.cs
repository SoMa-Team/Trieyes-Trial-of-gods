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
        if (!owner.isEnemy)
        {
            CharacterFactory.Instance.Deactivate(owner);   
        }
        else
        {
            EnemyFactory.Instance.Deactivate(owner);
        }
    }
}