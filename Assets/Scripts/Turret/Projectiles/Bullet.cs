using UnityEngine;
public class Bullet : Projectile
{
    
    protected Rigidbody rb;

    new BallisticCore turret;
    
    void Update()
    {
        if(body != null)
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

        DroneBlock droneBlock = other.gameObject.GetFirstComponentInHierarchy<DroneBlock>();
        if (droneBlock != null)
        {
            if(droneBlock.controller != null && droneBlock.controller.curTeam != originTeam)
                droneBlock.TakeDamage(Mathf.Pow(rb.velocity.magnitude, 2)* rb.mass);
        }
        
        Destroy(gameObject);   
    }
    
}
