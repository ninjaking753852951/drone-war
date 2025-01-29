using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtils;
public class DamageableManager : MonoBehaviour {
       
    List<IDamageable> registeredDamageables = new List<IDamageable>();

    public static DamageableManager Instance;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void RegisterDamageable(IDamageable damageable)
    {
        registeredDamageables.Add(damageable);
    }

    public void DeregisterDamageable(IDamageable damageable)
    {
        if (registeredDamageables.Contains(damageable))
        {
            registeredDamageables.Remove(damageable);
        }
        else
        {
            Debug.LogWarning("Tried to deregister a damageable not already registered");
        }
    }

    public List<IDamageable> FetchDamageables()
    {
        return registeredDamageables;
    }
    
}
