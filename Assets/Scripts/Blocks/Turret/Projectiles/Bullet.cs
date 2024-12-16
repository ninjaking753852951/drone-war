using UnityEngine;
public class Bullet : Projectile
{
    
    protected Rigidbody rb;

    public GameObject impactEffect;

    public VFXData vfxImpactEffect;
    
    new BallisticCore turret;
    
    void Update()
    {
        if(body != null && rb !=null && rb.velocity != Vector3.zero)
            body.rotation = Quaternion.LookRotation(rb.velocity);
    }
    
    public void Init(BallisticCore turret)
    {
        base.Init(turret);
        this.turret = turret;
        rb = GetComponent<Rigidbody>();
        rb.drag = turret.drag;
    }
    
    protected override void Hit(Collider other)
    {
        
        //SpawnImpactEffect(transform.position, Quaternion.LookRotation(rb.velocity * -1));
        VFXManager.instance.Spawn(vfxImpactEffect, transform.position, Quaternion.LookRotation(rb.velocity * -1), true);
        
        DroneBlock droneBlock = other.gameObject.GetFirstComponentInHierarchy<DroneBlock>();
        if (droneBlock != null)
        {
            if(droneBlock.controller != null && droneBlock.controller.curTeam != originTeam)
                droneBlock.TakeDamage(rb.velocity.magnitude* rb.mass * turret.DamageCalculation());
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
