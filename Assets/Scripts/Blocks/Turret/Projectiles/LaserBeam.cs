using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LaserBeam : Projectile
{
    public LineRenderer line;

    public VFXData impactEffect;

    LaserCore core;

    protected override void Hit(Collider other)
    {
        
    }
    
    public void Init(LaserCore turret)
    {
        base.Init(turret);
        core = turret;
        
        //Destroy(gameObject, turret.laserLifetime);
        Invoke(nameof(Deactivate), turret.laserLifetime);
        
        Transform firstHit = DamageScan();
        
        Vector3 endPoint = transform.position + transform.forward *turret.MaxRange();
        if (firstHit != null)
        {
            endPoint = firstHit.transform.position;
            SpawnImpactEffect(firstHit.transform.position, Quaternion.LookRotation(transform.position - firstHit.transform.position), turret.laserLifetime);
        }

        if (NetworkManager.Singleton.IsServer)
        {
            GetComponent<LaserNetworkHelper>().InitVisualRPC(endPoint);
        }
        else
        {
            ConfigureVisuals(endPoint);   
        }
    }

    public void ConfigureVisuals(Vector3 end)
    {
        line.SetPosition(0, transform.position);
        line.SetPosition(1, end);
    }

    void SpawnImpactEffect(Vector3 pos, Quaternion rot, float lifetime)
    {
        VFXManager.instance.Spawn(impactEffect, pos, rot);
        //GameObject impactEffectClone = Instantiate(impactEffect, pos, rot);

        /*ParticleSystem impactParticle = impactEffectClone.GetComponentInChildren<ParticleSystem>();
        var mainModule = impactParticle.main;
        mainModule.duration = lifetime;
        impactParticle.Play();
        Destroy(impactEffectClone, lifetime *2);*/
    }

    Transform DamageScan()
    {
        
        RaycastHit[] hits = Physics.RaycastAll(transform.position, transform.forward, core.MaxRange());
        List<Transform> hitTransforms = new List<Transform>();
        
        foreach (var hit in hits)      
        {
            DroneBlock droneBlock = hit.collider.gameObject.GetFirstComponentInHierarchy<DroneBlock>();

            if (droneBlock != null)
            {
                if(droneBlock.controller != null && droneBlock.controller.curTeam != turret.controller.curTeam)
                    hitTransforms.Add(hit.transform);
            }

            IDamageable damageable = hit.collider.gameObject.GetComponentInParent<IDamageable>();
            if(damageable != null && damageable.Team() != turret.controller.curTeam)
                hitTransforms.Add(hit.transform);

        }
        
        
        Transform firstHit = Utils.ClosestTo(hitTransforms, transform.position);
        if (firstHit != null)
        {
            IDamageable damageable = firstHit.GetComponentInParent<IDamageable>();
            if(damageable != null && damageable.Team() != turret.controller.curTeam)
                damageable.DealDamage();
            
            DroneBlock targetDroneBlock = firstHit.gameObject.GetFirstComponentInHierarchy<DroneBlock>();
            if (targetDroneBlock != null)
            {
                if(targetDroneBlock.controller != null)
                    targetDroneBlock.TakeDamage(core.DamageCalculation());
            }   
        }
        return firstHit;
    }
}
