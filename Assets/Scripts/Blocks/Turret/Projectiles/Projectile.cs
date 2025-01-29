using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected int originTeam = -1;
    protected Transform originRoot;

    public Transform body;

    public float waitForDespawnTime = 0.5f;
    
    protected TurretCoreController turret;

    ProjectilePoolManager.PooledTypes poolType;

    protected virtual void Start()
    {
        GameManager.Instance.onEnterBuildMode.AddListener(ReturnToPool);
    }

    void OnTriggerEnter(Collider other)
    {
        if(GameManager.Instance.IsOnlineAndClient())
            return;
        
        Projectile otherProjectile = other.transform.root.GetComponent<Projectile>();
        if (otherProjectile == null && other.transform.root != originRoot)
        {
            Hit(other);   
        }
    }

    protected abstract void Hit(Collider other);

    public virtual void Init(TurretCoreController turret)
    {
        this.turret = turret;
        poolType = this.turret.projectileType;
        originRoot = turret.controller.transform.root;
        originTeam = turret.controller.curTeam;
    }
    
    protected void Deactivate()
    {
        gameObject.SetActive(false);
        Invoke(nameof(ReturnToPool), waitForDespawnTime);
    }

    protected void ReturnToPool()
    {
        ProjectilePoolManager.Instance.ReturnObject(gameObject, poolType);
    }
}
