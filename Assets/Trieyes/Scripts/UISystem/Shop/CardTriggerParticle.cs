using System;
using NUnit.Framework.Constraints;
using PrimeTween;
using Unity.VisualScripting;
using UnityEngine;

public class CardTriggerParticle : MonoBehaviour
{
    [SerializeField] private RectTransform rectParticle;
    [SerializeField] private RectTransform rectTarget;
    
    [SerializeField] private ParticleSystem particle;
    private bool isActivated = false;
    
    public void Activate(RectTransform start, RectTransform end, float duration, float scale)
    {
        rectParticle.localScale *= (1 + scale);
        rectParticle.position = start.position;
        rectTarget.position = end.position;

        rectTarget.position += new Vector3(0, -end.sizeDelta.y / 2, 0);
        
        var main = particle.main;
        main.simulationSpeed = 0.5f / duration;
        main.startColor = Color.Lerp(new Color(0.3f, 0.3f, 0.8f), new Color(0.8f, 0.3f, 0.3f), scale);
        rectParticle.gameObject.SetActive(true);
        particle.Emit(1);
        
        isActivated = true;
    }

    private void Update()
    {
        if (isActivated && !particle.isPlaying && particle.particleCount == 0)
        {
            Deactivate();
        }
    }

    public void Deactivate()
    {
        if (gameObject.IsDestroyed())
            return;
        Destroy(gameObject);
    }
}
