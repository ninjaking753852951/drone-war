using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    protected int originTeam = -1;
    protected Transform originRoot;

    public Transform body;

    protected TurretCoreController turret;

    protected void Start()
    {
        GameManager.Instance.onEnterBuildMode.AddListener(() => Destroy(gameObject));
    }

    void OnTriggerEnter(Collider other)
    {
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

        originRoot = turret.controller.transform.root;
        originTeam = turret.controller.curTeam;
    }
}
