using UnityEngine;
using UnityUtils;
public class MissileCore : TurretCoreController
{
    
    public float missileAcceleration = 1;
    public float missileExplosionRadiusMultiplier = 1;
    public float missileDamage = 1000;


    public int simulationSafetyLimit = 500;
    public float simulationStepSize = 0.1f;
    

    public override void Shoot()
    {
        GameObject projectileClone = SpawnProjectile();

        Rigidbody rb = projectileClone.GetComponent<Rigidbody>();

        float projectileMass = projectileClone.GetComponent<Rigidbody>().mass;
        
        //mount.pitchRb.AddForceAtPosition(Vector3.up * projectileMass * shootVelocity * recoilMultiplier, mainBarrel.shootPoint.position);
        
        //rb.ResetInertiaTensor();
        rb.velocity = Vector3.zero;
        rb.AddForce(projectileClone.transform.forward * shootVelocity, ForceMode.VelocityChange);

        Missile projectile = projectileClone.GetComponent<Missile>();
        if (projectile != null)
        {
            projectile.Init(this);
        }  
    }
    
    public override float MaxRange()
    {
        float maxDistAngle = 45;
        return SimulateMissileArc(shootVelocity, maxDistAngle, missileAcceleration, 0);
    }
    public override float CalculateTargetPitchAngle(Vector3 targetPos, float interceptTime = -1)
    {
        float horizontalDistance = (targetPos - transform.position).With(y: 0).magnitude;
        targetPos.y -= shootHeightOffset;
        float launchAngle =
            CalculateMissileLaunchAngle(shootVelocity, missileAcceleration, new Vector2(horizontalDistance, targetPos.y));

        return launchAngle;
    }
    public override float EstimateTimeOfFlight(float initialVelocity, float distance)
    {
        float time = 0f;
        float velocity = initialVelocity;

        int safety = 500;
        
        while (distance > 0 && safety > 0)
        {
            safety--;
            float deltaDistance = velocity * Time.fixedDeltaTime;
            distance -= deltaDistance;
            velocity += missileAcceleration * simulationStepSize;
            time += Time.fixedDeltaTime;
        }

        return time;
    }

    float CalculateMissileLaunchAngle(float v0, float a, Vector2 targetPos)
    {
        float bestAngle = 0;
        float closestDistance = float.MaxValue;
        float tolerance = 0.5f;
        Vector2 angleLimits = new Vector2(90, 0);
        int safety = simulationSafetyLimit;

        // account for mount offset
        targetPos.y -= transform.position.y;

        while ((angleLimits.x - angleLimits.y) > 0.01f && safety > 0)
        {
            safety--;
            
            float theta = (angleLimits.x + angleLimits.y) / 2;
            float xDist = SimulateMissileArc(v0, theta, a, targetPos.y);
            float distanceError = Mathf.Abs(xDist - targetPos.x);

            if (distanceError < closestDistance)
            {
                closestDistance = distanceError;
                bestAngle = theta;
            }

            if (xDist > targetPos.x)
                angleLimits.x = theta;
            else
                angleLimits.y = theta;

            if (distanceError <= tolerance)
                break;
        }

        return bestAngle;
    }
    
    float SimulateMissileArc(float initialVelocity, float angle, float acceleration, float targetHeight)
    {
        float xDist = 0;
        float yDist = 0;
        bool pastApex = false;
        

        Vector2 velocity =
            new Vector2(initialVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), initialVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

        int safety = simulationSafetyLimit;
        while (safety > 0)
        {
            if(yDist <= targetHeight && pastApex)
                break;
            
            pastApex = (velocity.y * simulationStepSize < 0); // we are past the apex

    
            yDist += velocity.y * simulationStepSize;
            xDist += velocity.x * simulationStepSize;

            velocity += velocity.normalized * acceleration * simulationStepSize;
            velocity += Vector2.up * simulationStepSize * Physics.gravity.y;

            Vector3 rayPos = transform.position + YawRotation()* new Vector3(0, yDist, xDist);
            Vector3 velocityRay = YawRotation()* new Vector3(0, velocity.y, velocity.x);
            Debug.DrawLine( rayPos, rayPos + velocityRay * simulationStepSize, Color.gray,0.2f);
                
            safety--;
            if(safety <= 0)
                Debug.Log("HIT SAFETY LIMIT FOR SIMULATION");
                
        }

        return xDist;
    }
}
