using System;
using System.Collections.Generic;
using ImprovedTimers;
using Misc;
using Unity.Mathematics;
using UnityEngine;
using UnityUtils;
public class Missile : Projectile, IDamageable
{
        
    
    public GameObject impactEffect;
    public float explosionRadius;

    public int trackingUpdateFrequency = 5;

    public float trackingAngularSpeed = 30;
    
    new MissileCore turret;
    
    float missileAcceleration;
    
    bool hasDetonated = false;
    public Transform target;
    Rigidbody rb;
    FrequencyTimer trackingTimer;

    protected override void Start()
    {
        base.Start();
        
        rb = GetComponent<Rigidbody>();
    }
    
    void OnDisable()
    {
        if(trackingTimer != null)
            trackingTimer.Dispose();
        DeregisterDamageable();
    }

    void OnDestroy()
    {
        if(trackingTimer != null)
            trackingTimer.Dispose();
    }

    void FixedUpdate()
    {
        if(rb != null)
            rb.AddForce(rb.velocity.normalized * ( missileAcceleration * Time.fixedDeltaTime ), ForceMode.VelocityChange);
    }
    
    void Update()
    {
        if(body != null && rb !=null && rb.velocity != Vector3.zero)
            body.rotation = Quaternion.LookRotation(rb.velocity);
    }
    
    public void Init(MissileCore turret)
    {
        base.Init(turret);
        this.target = turret.target;
        this.turret = turret;
        rb = GetComponent<Rigidbody>();
        rb.drag = 0;
        missileAcceleration = turret.missileAcceleration;
        trackingTimer = new FrequencyTimer(trackingUpdateFrequency);
        trackingTimer.Start();
        trackingTimer.OnTick += UpdateTrajectory;
        hasDetonated = false;
        
        RegisterDamageable();
    }

    protected override void Hit(Collider other)
    {
        if(hasDetonated)
            return;
        hasDetonated = true;
        Detonate();
        
        Deactivate();
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

    void UpdateTrajectory()
    {
        if(transform == null || target == null)
            return;
        
        float xVelocity = rb.velocity.With(y: 0).magnitude;
        float xDist = (target.position - transform.position).With(y:0).magnitude;
        float yDist = target.position.y - transform.position.y;
        Vector2 targetPos2D = new Vector2(xDist, target.position.y);
        
        Vector3 horizontalTargetDirection = (target.position - transform.position).With(y:0).normalized;
        Vector3 horizontalVelocity = rb.velocity.With(y:0).normalized;

        float yawAngleError = Vector3.SignedAngle(horizontalVelocity, horizontalTargetDirection, Vector3.up);
        
        float verticalAngle = CalculateBallisticLaunchAngle(rb.velocity.magnitude, missileAcceleration, targetPos2D, 0 * Mathf.Rad2Deg);
        float currentVerticalAngle = Mathf.Asin(rb.velocity.y / rb.velocity.magnitude) * Mathf.Rad2Deg;
        float pitchAngleError = currentVerticalAngle - verticalAngle;
        
        Vector3 forward = rb.velocity.normalized;
        Vector3 pitchAxis = Vector3.Cross(forward, Vector3.up).normalized;

        Quaternion verticalRotation = Quaternion.AngleAxis(-pitchAngleError, pitchAxis);
        Quaternion horizontalRotation = Quaternion.AngleAxis(yawAngleError, Vector3.up);
        Quaternion targetRotation = horizontalRotation * verticalRotation;
        Quaternion correctionRotation = Quaternion.RotateTowards(Quaternion.identity, targetRotation, trackingAngularSpeed / trackingUpdateFrequency);
        rb.velocity = correctionRotation * rb.velocity;


        //DEBUG RAY
        Vector3 velocityRay = correctionRotation * rb.velocity.normalized;
        Debug.DrawLine( transform.position, transform.position + velocityRay * 5, Color.red,1/(float)trackingUpdateFrequency);
    }

    
    float CalculateBallisticLaunchAngle(float v0, float a, Vector2 targetPos, float estimate)
    {
        float bestAngle = 0;
        float closestDistance = float.MaxValue;
        float tolerance = 0.5f;

        float estimateLimits = 45;
        //float estimate = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg;
        
        /*Vector2 angleLimits = new Vector2(90, -90);*/
        Vector2 angleLimits = new Vector2(estimate + estimateLimits, estimate - estimateLimits);
        int safety = turret.simulationSafetyLimit;

        targetPos.y -= transform.position.y;
        
        while ((angleLimits.x - angleLimits.y) > 0.01f && safety > 0)
        {
            safety--;
            
            float theta = (angleLimits.x + angleLimits.y) / 2;
            float yDist = SimulateBallisticArc(v0, theta, missileAcceleration, targetPos.x);
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

    float SimulateBallisticArc(float initialVelocity, float angle, float acceleration, float targetDist)
    {
        float xDist = 0;
        float yDist = 0;
        
        
        
        Vector2 velocity =
            new Vector2(initialVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), initialVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

        int safety = turret.simulationSafetyLimit;
        while (safety > 0)
        {
            if(xDist > targetDist|| velocity.magnitude < initialVelocity/2)
                break;

            /*bool goingDownwards = velocity.y < 0;
            bool targetIsAbove = targetHeight - yDist > 0;
            
            if(goingDownwards && targetIsAbove || !goingDownwards && !targetIsAbove)
                break;*/
            
            yDist += velocity.y * turret.simulationStepSize;
            xDist += velocity.x * turret.simulationStepSize;


            velocity += velocity.normalized * acceleration * turret.simulationStepSize;
            velocity += Vector2.up * turret.simulationStepSize * Physics.gravity.y;
            
            Vector3 rayPos = transform.position + Quaternion.LookRotation(transform.forward.With(y:0))* new Vector3(0, yDist, xDist);
            Vector3 velocityRay = Quaternion.LookRotation(transform.forward.With(y:0))* new Vector3(0, velocity.y, velocity.x);
            Debug.DrawLine( rayPos, rayPos + velocityRay * turret.simulationStepSize, Color.yellow,1/(float)trackingUpdateFrequency);


            safety--;
            if(safety <= 0)
                Debug.Log("HIT SAFETY LIMIT FOR SIMULATION");
                
        }

        return yDist;
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
    public TargetTypes TargetType()
    {
        return TargetTypes.Missile;
    }
}
