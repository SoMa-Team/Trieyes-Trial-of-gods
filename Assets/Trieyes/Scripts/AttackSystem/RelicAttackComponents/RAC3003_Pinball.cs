using AttackComponents;
using AttackSystem;
using BattleSystem;
using CharacterSystem;
using PrimeTween;
using UnityEngine;

public class RAC3003_Pinball : AttackComponent
{
    [SerializeField] private ParticleSystem particleSystem;
    private Vector3 direction;
    private Camera camera;
    private const float speed = 8f;
    
    public override void Activate(Attack attack, Vector2 direction)
    {
        base.Activate(attack, direction);
        
        particleSystem.Play();
        Tween.Delay(3f).OnComplete(() =>
        {
            particleSystem.time = 3f;
            particleSystem.Pause();
        });
        
        this.direction = new Vector3(1, 1, 0).normalized;
    }

    public override void Deactivate()
    {
        particleSystem.Stop();
        camera = null;
        
        base.Deactivate();
    }

    protected override void Update()
    {
        base.Update();

        if (camera == null)
            camera = Camera.main;
        
        var nextPosition = attack.transform.position + (speed * Time.deltaTime) * direction;
        var zDist = Mathf.Abs(transform.position.z - camera.transform.position.z);
        
        Vector3 bottomLeft = camera.ViewportToWorldPoint(new Vector3(0, 0, zDist));
        Vector3 topRight = camera.ViewportToWorldPoint(new Vector3(1, 1, zDist));
        
        if (nextPosition.x < bottomLeft.x || topRight.x < nextPosition.x)
        {
            var width = topRight.x - bottomLeft.x;
            
            nextPosition.x = Mathf.Abs((nextPosition.x - bottomLeft.x) % (2 * width));
            nextPosition.x = Mathf.Min(nextPosition.x, 2 * width - nextPosition.x) + bottomLeft.x;
            direction.x *= -1;
        }
        
        if (nextPosition.y < bottomLeft.y || topRight.y < nextPosition.y)
        {
            var height = topRight.y - bottomLeft.y;
            
            nextPosition.y = Mathf.Abs((nextPosition.y - bottomLeft.y) % (2 * height));
            nextPosition.y = Mathf.Min(nextPosition.y, 2 * height - nextPosition.y) + bottomLeft.y;
            direction.y *= -1;
        }
        
        attack.transform.position = nextPosition;
    }
    
    public override void ProcessComponentCollision(Pawn targetPawn)
    {
        base.ProcessComponentCollision(targetPawn);

        DamageProcessor.ProcessHit(attack, targetPawn);
    }
}
