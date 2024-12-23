using UnityEngine;
public class Bullet : Projectile
{
    
    protected Rigidbody rb;

    public GameObject impactEffect;

    public VFXData vfxImpactEffect;
    
    new BallisticCore bulletTurret;
    
    void Update()
    {
        if(body != null && rb !=null && rb.linearVelocity != Vector3.zero)
            body.rotation = Quaternion.LookRotation(rb.linearVelocity);
    }
    
    public void Init(BallisticCore turret)
    {
        base.Init(turret);
        this.bulletTurret = turret;
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = turret.drag;
    }
    
    protected override void Hit(Collider other)
    {
        
        //SpawnImpactEffect(transform.position, Quaternion.LookRotation(rb.velocity * -1));
        VFXManager.instance.Spawn(vfxImpactEffect, transform.position, Quaternion.LookRotation(rb.linearVelocity * -1), true);
        
        DroneBlock droneBlock = other.gameObject.GetFirstComponentInHierarchy<DroneBlock>();
        if (droneBlock != null)
        {
            if(droneBlock.controller != null && droneBlock.controller.curTeam != originTeam)
                droneBlock.TakeDamage(rb.linearVelocity.magnitude* rb.mass * bulletTurret.DamageCalculation());
        }
        
        Deactivate();
    }
    
    void SpawnImpactEffect(Vector3 pos, Quaternion rot)
    {
        GameObject impactEffectClone = Instantiate(impactEffect, pos, rot);

        ParticleSystem impactParticle = impactEffectClone.GetComponentInChildren<ParticleSystem>();
        var mainModule = impactParticle.main;
        Destroy(impactEffectClone, mainModule.duration);
    }
    
}
