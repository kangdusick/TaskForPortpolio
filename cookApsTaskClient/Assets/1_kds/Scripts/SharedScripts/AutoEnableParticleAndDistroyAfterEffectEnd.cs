using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoEnableParticleAndDistroyAfterEffectEnd : MonoBehaviour
{
    ParticleSystem particle;

    private void Awake()
    {
        particle = transform.GetComponent<ParticleSystem>();
    }
    public void Init(Color color)
    {
        particle.startColor = color;
    }
    private void OnEnable()
    {
        particle.Play(true);
        PoolableManager.Instance.Destroy(particle.gameObject, particle.main.duration);
    }
}
