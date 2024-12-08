using System;
using System.Collections.Generic;
using UnityEngine;
public class Missile : Projectile, IDamageable
{
        
    
    public GameObject impactEffect;
    public float explosionRadius;

    new MissileCore turret;
    
    float missileAcceleration;
    
    Rigidbody rb;

    void Start()
    {
        RegisterDamageable();
    }
    
    void OnDisable()
    {
        DeregisterDamageable();
    }

    void FixedUpdate()
    {
        if(rb != null)
            rb.AddForce(rb.velocity.normalized * ( missileAcceleration * Time.fixedDeltaTime ), ForceMode.VelocityChange);
    }
    
    void Update()
    {
        if(body != null && rb !=null)
            body.rotation = Quaternion.LookRotation(rb.velocity);
    }
    
    public void Init(MissileCore turret)
    {
        base.Init(turret);
        this.turret = turret;
        rb = GetComponent<Rigidbody>();
        rb.drag = 0;
        missileAcceleration = turret.missileAcceleration;
    }

    protected override void Hit(Collider other)
    {
        Detonate();
        Destroy(gameObject);   
    }

    float ExplosionRadius()
    {
        if (turret != null)
        {
            return explosionRadius * turret.missileExplosionRadiusMultiplier;
        }
        else
        {
            return explosionRadius;
        }
    }
    
    void Detonate()
    {
        Instantiate(impactEffect, transform.position, Quaternion.identity);
        
        Collider[] hits = Physics.OverlapSphere(transform.position, ExplosionRadius());
        
        if(hits == null)
            return;
        
        List<DroneController> controllers = new List<DroneController>();
        
        foreach (var hit in hits)      
        {
            DroneBlock droneBlock = hit.gameObject.GetFirstComponentInHierarchy<DroneBlock>();

            if (droneBlock != null)
            {
                if(droneBlock.controller != null && droneBlock.controller.curTeam != turret.controller.curTeam)
                {
                    if (!controllers.Contains(droneBlock.controller))
                    {
                        controllers.Add(droneBlock.controller);
                        droneBlock.TakeDamage(turret.missileDamage);      
                    }
                    //droneBlock.TakeDamage(turret.laserDamage);  
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    public void DealDamage()
    {
        Debug.Log("LASERED A MISSILE");
        Detonate();
        Destroy(gameObject);   
    }
    public int Team()
    {
        return turret.controller.curTeam;
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
}
