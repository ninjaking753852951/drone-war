using UnityEngine;
public class LaserCore : TurretCoreController
{
    
    public float laserLifetime = 0.5f;
    //public float laserDamage = 100;
    public float laserRange = 30;


    public override void Shoot()
    {
        GameObject projectileClone = SpawnProjectile();
        
        LaserBeam laserBeam = projectileClone.GetComponent<LaserBeam>();
        if (laserBeam != null)
        {
            laserBeam.Init(this);
        }
    }
    public override float MaxRange()
    {
        return laserRange * rangeMultiplier;
    }
    public override float CalculateTargetPitchAngle(Vector3 targetPos, float interceptTime = -1)
    {
        Vector3 shootPoint = transform.position;
        
        Vector3 directionToTarget = targetPos - shootPoint;

        float pitchAngle = Mathf.Atan2(directionToTarget.y, 
            new Vector2(directionToTarget.x, directionToTarget.z).magnitude) * Mathf.Rad2Deg;

        return pitchAngle;
    }
    public override float EstimateTimeOfFlight(float initialVelocity, float distance)
    {
        return 0;
    }
    public override float DamageCalculation()
    {
        return baseDamage * damageMultiplier;
    }
}
