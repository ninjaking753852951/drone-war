using UnityEngine;
using UnityUtils;
public class BallisticCore : TurretCoreController
{
    
    public int simulationSafetyLimit = 500;
    public float simulationStepSize = 0.1f;

    public float deviance = 1;

    public float accuracyTolerance = 0.5f;

    public GameObject shootEffect;
    public float damageMultiplier;

    public float drag = 0.3f;         // Unity-style drag coefficient

    public override void Shoot()
    {
        GameObject projectileClone = Instantiate(projectilePrefab, mainBarrel.shootPoint.position, mainBarrel.shootPoint.rotation);

        Rigidbody rb = projectileClone.GetComponent<Rigidbody>();

        float projectileMass = projectileClone.GetComponent<Rigidbody>().mass;
        
        mount.pitchRb.AddForceAtPosition(Vector3.up * projectileMass * shootVelocity * recoilMultiplier, mainBarrel.shootPoint.position);
        mount.pitchRb.AddForce(mainBarrel.shootPoint.forward * projectileMass * shootVelocity * recoilMultiplier * -1);
        
        rb.AddForce(projectileClone.transform.forward * shootVelocity + Random.insideUnitSphere * deviance, ForceMode.VelocityChange);

        Instantiate(shootEffect, mainBarrel.shootPoint);
        
        Bullet projectile = projectileClone.GetComponent<Bullet>();
        if (projectile != null)
        {
            projectile.Init(this);
        }    
    }
    
    public override float MaxRange()
    {
        float maxDistAngle = 45;
        return SimulateBallisticArcTargetY(shootVelocity, maxDistAngle, drag, 0);
    }
    public override float CalculateTargetPitchAngle(Vector3 targetPos, float interceptTime = -1)
    {
        float horizontalDistance = (targetPos - transform.position).With(y: 0).magnitude;
        targetPos.y -= shootHeightOffset;
        float launchAngle =
            CalculateBallisticLaunchAngle(shootVelocity, drag, new Vector2(horizontalDistance, targetPos.y));

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
            velocity *= (1 - drag * Time.fixedDeltaTime);
            time += Time.fixedDeltaTime;
        }

        return time;
    }
    
    float CalculateBallisticLaunchAngle(float v0, float a, Vector2 targetPos)
    {
        float bestAngle = 0;
        float closestDistance = float.MaxValue;
        float tolerance = accuracyTolerance;

        float estimateLimits = 30;
        float estimate = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
        
        /*Vector2 angleLimits = new Vector2(90, -90);*/
        Vector2 angleLimits = new Vector2(estimate + estimateLimits, estimate - estimateLimits);
        int safety = simulationSafetyLimit;

        targetPos.y -= transform.position.y;
        

        while ((angleLimits.x - angleLimits.y) > 0.01f && safety > 0)
        {
            safety--;
            
            float theta = (angleLimits.x + angleLimits.y) / 2;
            float yDist = SimulateBallisticArc(v0, theta, drag, targetPos.x);
            float distanceError = Mathf.Abs(yDist - targetPos.y);

            if (distanceError < closestDistance)
            {
                closestDistance = distanceError;
                bestAngle = theta;
            }

            if (yDist > targetPos.y)
                angleLimits.x = theta;
            else
                angleLimits.y = theta;

            if (distanceError <= tolerance)
                break;
        }

        return bestAngle;
        
    }

    float SimulateBallisticArc(float initialVelocity, float angle, float drag, float targetDist)
    {
        float xDist = 0;
        float yDist = 0;
        
        
        
        Vector2 velocity =
            new Vector2(initialVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), initialVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

        int safety = simulationSafetyLimit;
        while (safety > 0)
        {
            if(xDist > targetDist|| velocity.magnitude < initialVelocity/2)
                break;

            /*bool goingDownwards = velocity.y < 0;
            bool targetIsAbove = targetHeight - yDist > 0;
            
            if(goingDownwards && targetIsAbove || !goingDownwards && !targetIsAbove)
                break;*/
            
            yDist += velocity.y * simulationStepSize;
            xDist += velocity.x * simulationStepSize;

            velocity *= (1- drag* simulationStepSize);
            velocity += Vector2.up * simulationStepSize * Physics.gravity.y;

            Vector3 rayPos = transform.position + mount.yawRb.transform.rotation * new Vector3(0, yDist, xDist);
            Vector3 velocityRay = mount.yawRb.transform.rotation* new Vector3(0, velocity.y, velocity.x);
            Debug.DrawLine( rayPos, rayPos + velocityRay * simulationStepSize, Color.gray,0.2f);
                
   
            
            safety--;
            if(safety <= 0)
                Debug.Log("HIT SAFETY LIMIT FOR SIMULATION");
                
        }

        return yDist;
    }
    
    float SimulateBallisticArcTargetY(float initialVelocity, float angle, float drag, float targetDist)
    {
        float xDist = 0;
        float yDist = 0;
        
        
        
        Vector2 velocity =
            new Vector2(initialVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), initialVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

        int safety = simulationSafetyLimit;
        while (safety > 0)
        {
            if(yDist < targetDist|| velocity.magnitude < initialVelocity/2)
                break;

            /*bool goingDownwards = velocity.y < 0;
            bool targetIsAbove = targetHeight - yDist > 0;

            if(goingDownwards && targetIsAbove || !goingDownwards && !targetIsAbove)
                break;*/
            
            yDist += velocity.y * simulationStepSize;
            xDist += velocity.x * simulationStepSize;

            velocity *= (1- drag* simulationStepSize);
            velocity += Vector2.up * simulationStepSize * Physics.gravity.y;

            Vector3 rayPos = transform.position + mount.yawRb.transform.rotation * new Vector3(0, yDist, xDist);
            Vector3 velocityRay = mount.yawRb.transform.rotation* new Vector3(0, velocity.y, velocity.x);
            Debug.DrawLine( rayPos, rayPos + velocityRay * simulationStepSize, Color.gray,0.2f);
                
            
            safety--;
            if(safety <= 0)
                Debug.Log("HIT SAFETY LIMIT FOR SIMULATION");
                
        }

        return xDist;
    }
}
