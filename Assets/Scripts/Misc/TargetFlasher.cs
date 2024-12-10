using System;
using System.Collections;
using Misc;
using UnityEngine;

public class TargetFlasher : MonoBehaviour, IDamageable
{
    public Renderer rend;
    private Color originalColor;

    void Start()
    {
        RegisterDamageable();
        
        if(GameManager.Instance.currentGameMode == GameMode.Battle)
            Destroy(gameObject);
        
        // Store the original color of the material
        if (rend != null)
        {
            originalColor = rend.material.color;
        }
        
    }

    void OnDisable()
    {
        DeregisterDamageable();
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the hit object contains a Projectile script
        if (other.transform.root.GetComponent<Projectile>() != null)
        {
            // If so, set the material color to red for 0.2 seconds
            StartCoroutine(FlashRed());
        }
    }

    private IEnumerator FlashRed()
    {
        if (rend != null)
        {
            // Change material color to red
            rend.material.color = Color.red;

            // Wait for 0.2 seconds
            yield return new WaitForSeconds(0.2f);

            // Reset material color to the original
            rend.material.color = originalColor;
        }
    }
    public void DealDamage()
    {
        
    }
    public int Team()
    {
        return -1;
    }
    public Transform Transform()
    {
        return transform;
    }
    public void RegisterDamageable()
    {
        DamageableManager.Instance.RegisterDamageable(this);
    }
    public void DeregisterDamageable()
    {
        if(DamageableManager.Instance != null)
            DamageableManager.Instance.DeregisterDamageable(this);
    }
    public TargetTypes TargetType()
    {
        return TargetTypes.Debug;
    }
}
