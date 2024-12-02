using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityUtils;

public class TurretMountController : MonoBehaviour
{
    public Transform aimPoint;

    public HingeJoint yawJoint;
    public HingeJoint pitchJoint;

    public float readyToFirePositionThreshold = 5;
    public float readyToFireDelta = 0.1f;
    public float aimForce = 100;
    public float maxTurnSpeed = 10;
    public float nearTargetSmoothing = 1;
    
    public bool usePredictiveAiming;
    
    [Header("Missile Settings")]
    public float simulationStepSize = 0.1f;
    [FormerlySerializedAs("safetyLimitDebug")] public int simulationSafetyLimit = 500;
    
    public float debugPitch;
    
    TurretCoreController turret;
    
    float targetPitchAngle;
    float targetYawAngle;

    float pitchDelta;
    float pitchPos;

    float yawDelta;
    float yawPos;
    
    Rigidbody yawRb;
    Rigidbody pitchRb;

    Vector3 targetPosition;

    void Awake()
    {
        yawRb = yawJoint.GetComponent<Rigidbody>();
        yawRb.isKinematic = true;
        yawRb.useGravity = false;
        
        pitchRb = pitchJoint.GetComponent<Rigidbody>();
        pitchRb.isKinematic = true;
        pitchRb.useGravity = false;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        pitchDelta = pitchPos - pitchJoint.angle;
        pitchPos = pitchJoint.angle;

        yawDelta = yawPos - yawJoint.angle;
        yawPos = yawJoint.angle;
    }

    void Update()
    {
        SetPitchAngle(targetPitchAngle);
        SetYawAngle(targetYawAngle);
    }


    public void Deploy(TurretCoreController controller)
    {
        turret = controller;

        yawRb.isKinematic = false;
        yawRb.useGravity = true;
        
        pitchRb.isKinematic = false;
        pitchRb.useGravity = true;

        yawJoint.connectedBody = Utils.FindParentRigidbody(transform, yawRb);
        InitJointMotors();
    }

    public void UpdateTurretAim(TurretCoreController turret, Vector3 targetPos, Vector3 targetVelocity)
    {
        float interceptTime = EstimateInterceptTime(turret, targetPos, targetVelocity);
        Vector3 predictedTargetPos = targetPos + targetVelocity * interceptTime;

        UpdateYawAngle(turret, predictedTargetPos);
        
        switch (turret.turretType)
        {
            case TurretCoreController.TurretType.Ballistic:
                UpdatePitchAngle(turret, predictedTargetPos, interceptTime);
                break;
            case TurretCoreController.TurretType.Missile:
                UpdatePitchAngleMissile(predictedTargetPos);
                break;
            case TurretCoreController.TurretType.Laser:
                UpdatePitchAngleLaser(targetPos);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    void UpdatePitchAngleLaser(Vector3 targetPos)
    {
        float horizontalDistance = (targetPos - transform.position).With(y: 0).magnitude;

        float launchAngle =
            CalculateLaserPitchAngle(targetPos);

        debugPitch = launchAngle;

        targetPitchAngle = -launchAngle;
    }

    float CalculateLaserPitchAngle(Vector3 targetPos)
    {
        //TODO
        return 0;
    }

    void UpdatePitchAngleMissile(Vector3 targetPos)
    {
        float horizontalDistance = (targetPos - transform.position).With(y: 0).magnitude;

        float launchAngle =
            CalculateMissileLaunchAngle(turret.shootVelocity, turret.missileAcceleration, new Vector2(horizontalDistance, targetPos.y));

        debugPitch = launchAngle;

        targetPitchAngle = -launchAngle;
    }

    public float CalculateMaxRange(TurretCoreController turret)
    {
        float maxDistAngle = 45;

        float maxRange = 0;

        switch (turret.turretType)
        {
            case TurretCoreController.TurretType.Ballistic:
                maxRange = SimulateBallisticArc(turret.shootVelocity, maxDistAngle, turret.drag, 0);
                Debug.Log("correct");
                break;
            case TurretCoreController.TurretType.Missile:
                maxRange = SimulateMissileArc(turret.shootVelocity, maxDistAngle, turret.missileAcceleration, 0);
                break;
            case TurretCoreController.TurretType.Laser:
                maxRange = turret.laserRange;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Debug.Log(maxRange);
        return maxRange;
    }
    
    float CalculateMissileLaunchAngle(float v0, float a, Vector2 targetPos)
    {
        float bestAngle = 0;
        float closestDistance = float.MaxValue;
        float tolerance = 0.5f;
        Vector2 angleLimits = new Vector2(90, 0);
        int safety = simulationSafetyLimit;

        // account for mount offset
        targetPos.y -= 1;

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
    
    float SimulateBallisticArc(float initialVelocity, float angle, float drag, float targetHeight)
    {
        float xDist = 0;
        float yDist = 0;
        
        Vector2 velocity =
            new Vector2(initialVelocity * Mathf.Cos(angle * Mathf.Deg2Rad), initialVelocity * Mathf.Sin(angle * Mathf.Deg2Rad));

        int safety = simulationSafetyLimit;
        while (safety > 0)
        {
            if(yDist < targetHeight)
                break;
            
            yDist += velocity.y * simulationStepSize;
            xDist += velocity.x * simulationStepSize;

            velocity *= (1- drag* simulationStepSize);
            velocity += Vector2.up * simulationStepSize * Physics.gravity.y;

            Vector3 rayPos = transform.position + yawRb.transform.rotation* new Vector3(0, yDist, xDist);
            Vector3 velocityRay = yawRb.transform.rotation* new Vector3(0, velocity.y, velocity.x);
            Debug.DrawLine( rayPos, rayPos + velocityRay * simulationStepSize, Color.gray,0.2f);
                
            safety--;
            if(safety <= 0)
                Debug.Log("HIT SAFETY LIMIT FOR SIMULATION");
                
        }

        return xDist;
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

            Vector3 rayPos = transform.position + yawRb.transform.rotation* new Vector3(0, yDist, xDist);
            Vector3 velocityRay = yawRb.transform.rotation* new Vector3(0, velocity.y, velocity.x);
            Debug.DrawLine( rayPos, rayPos + velocityRay * simulationStepSize, Color.gray,0.2f);
                
            safety--;
            if(safety <= 0)
                Debug.Log("HIT SAFETY LIMIT FOR SIMULATION");
                
        }

        return xDist;
    }
    
    void UpdatePitchAngle(TurretCoreController turret, Vector3 predictedTargetPos, float interceptTime)
    {
        Vector3 directionToTarget = predictedTargetPos - aimPoint.position;
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);
        float horizontalDistance = horizontalDirection.magnitude;

        float verticalDrop = SimulateVerticalDrop(turret.shootVelocity, interceptTime);

        verticalDrop -= 0; // to account for offset
        
        float adjustedTargetY = directionToTarget.y + verticalDrop;
        targetPitchAngle = Mathf.Atan2(adjustedTargetY, horizontalDistance) * Mathf.Rad2Deg * -1;

    }

    void SetPitchAngle(float targetPitchAngle)
    {
        float currentAngle = pitchJoint.angle;
        float angleError = targetPitchAngle - currentAngle;

        angleError = Mathf.Clamp(angleError/nearTargetSmoothing, -1, 1);
        
        JointMotor pitchMotor = pitchJoint.motor;
        pitchMotor.targetVelocity = angleError * maxTurnSpeed;
        pitchJoint.motor = pitchMotor;
    }

    void InitPitchJoint()
    {
        JointMotor pitchMotor = pitchJoint.motor;
        pitchMotor.force = aimForce;
        pitchJoint.motor = pitchMotor;
        pitchJoint.useMotor = true;
    }


    private float EstimateInterceptTime(TurretCoreController turret, Vector3 targetPos, Vector3 targetVelocity)
    {
        float projectileVelocity = turret.shootVelocity;
        Vector3 turretPosition = aimPoint.position;

        float time = 0f;
        const float tolerance = 0.01f;
        const int maxIterations = 100;

        for (int i = 0; i < maxIterations; i++)
        {
            Vector3 predictedTargetPos = targetPos + targetVelocity * time;
            float horizontalDistance = (predictedTargetPos - turretPosition).magnitude;

            float newTime = EstimateTimeOfFlight(projectileVelocity, horizontalDistance);
            if (Mathf.Abs(newTime - time) < tolerance)
                return newTime;

            time = newTime;
        }

        return time;
    }

    private float EstimateTimeOfFlight(float initialVelocity, float distance)
    {
        float time = 0f;
        float velocity = initialVelocity;

        int safety = 500;
        
        while (distance > 0 && safety > 0)
        {
            safety--;
            float deltaDistance = velocity * Time.fixedDeltaTime;
            distance -= deltaDistance;
            velocity *= (1 - turret.drag * Time.fixedDeltaTime);
            time += Time.fixedDeltaTime;
        }

        return time;
    }

    private float SimulateVerticalDrop(float initialVelocity, float timeOfFlight)
    {
        float verticalVelocity = 0f;
        float verticalDrop = 0f;
        float gravity = Physics.gravity.y;

        for (float t = 0; t < timeOfFlight; t += Time.fixedDeltaTime)
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
            verticalVelocity *= (1 - turret.drag * Time.fixedDeltaTime);
            verticalDrop += verticalVelocity * Time.fixedDeltaTime;
        }

        return Mathf.Abs(verticalDrop);
    }


    void UpdateYawAngle(TurretCoreController turret, Vector3 targetPos)
    {
        // Calculate the direction from the aimPoint to the target
        Vector3 directionToTarget = targetPos - aimPoint.position;
        
        // Create a horizontal direction vector by ignoring the Y component
        Vector3 horizontalDirection = new Vector3(directionToTarget.x, 0, directionToTarget.z);

        // Calculate the yaw angle (angle around the Y-axis)
        targetYawAngle = Mathf.Atan2(horizontalDirection.x, horizontalDirection.z) * Mathf.Rad2Deg;

        // account for base rotation
        targetYawAngle = Mathf.Repeat(targetYawAngle - aimPoint.rotation.eulerAngles.y + 180, 360) - 180;

    }
    
    void SetYawAngle(float targetYawAngle)
    {
        float currentAngle = yawJoint.angle;
        float angleError = targetYawAngle - currentAngle;
        
        // to prevent acceleration towards target point and oscilation
        angleError = Mathf.Clamp(angleError, -1, 1);
        
        JointMotor yawMotor = yawJoint.motor;
        yawMotor.targetVelocity = angleError * maxTurnSpeed; // Adjust multiplier for speed control
        yawJoint.motor = yawMotor;
    }

    void InitJointMotors()
    {
        InitYawJoint();
        InitPitchJoint();
    }

    void InitYawJoint()
    {
        JointMotor yawMotor = yawJoint.motor;
        yawMotor.force = aimForce;
        yawJoint.motor = yawMotor;
        yawJoint.useMotor = true;
    }


    public bool ReadyToFire()
    {
        float maxAngle = GetMaxAllowableAngle(transform.position, targetPosition, readyToFirePositionThreshold);

        Vector2 curAngles = new Vector2(yawJoint.angle, pitchJoint.angle);
        Vector2 curTargetAngles = new Vector2(targetYawAngle, targetPitchAngle);

        Vector2 angleError = curAngles - curTargetAngles;
        Vector2 angleDelta = new Vector2(pitchDelta, yawDelta);

        return angleError.magnitude < maxAngle && angleDelta.magnitude < readyToFireDelta;
    }
    
    float GetMaxAllowableAngle(Vector3 P1, Vector3 P2, float tolerance)
    {
        float distance = Vector3.Distance(P1, P2);
        if (distance <= tolerance) return 180f; // Covers all directions
        return Mathf.Asin(tolerance / distance) * Mathf.Rad2Deg;
    }

    

}
