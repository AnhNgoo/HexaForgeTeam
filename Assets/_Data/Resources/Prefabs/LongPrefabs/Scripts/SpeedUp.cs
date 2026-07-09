using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    [Header("Speed Up")] 
    [SerializeField] private float speed = 5f;
    [SerializeField] private float duration = 5f;

    [Header("Effect")]
    [SerializeField] private GameObject effectPrefab;
    [SerializeField] private ParticleSystem[] particleSystems;

    private Collider myCollider;
    private void Awake()
    {
        myCollider = GetComponent<Collider>();
        particleSystems = GetComponentsInChildren<ParticleSystem>(true); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ISpeedUp iSpeedUp = other.GetComponent<ISpeedUp>();
            if (iSpeedUp != null)
            {
                CharacterStats characterStats = new CharacterStats()
                {
                    speed = this.speed,
                };
                iSpeedUp.SpeedUp(characterStats);
            }
            if (effectPrefab != null)
            {
               GameObject effect = Instantiate(effectPrefab);

                effect.transform.SetParent(other.transform);
                effect.transform.localPosition = new Vector3(0f, -1.5f, 0f);
                effect.transform.localRotation = Quaternion.identity;

                Destroy(effect, duration);
            }
            StartCoroutine(Respawn());
        }
    }
    IEnumerator Respawn()
    {
        myCollider.enabled = false;
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        
        yield return new WaitForSeconds(30);

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }
        myCollider.enabled = true;
        
    }
}
