using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class DroneController : MonoBehaviour
{
    
    [Header("Steering Settings")]
    
    public float currentSteer; // ranges from -180 to 180
    public float steerMultiplier;
    public float safeTurnAngleMultiplier; // lower means slower cornering, higher means higher chance of rollover
    
    [Header("Motor Settings")]
    
    public float motorTorque; 
    public float comHeight;
    public float trackWidth;
    public float wheelBase;
    public float mass;
    
    [Header("Outline Settings")]
    
    public Outline outline;
    public float selectionWidth = 2;

    [Header("Misc Settings")] 
    
    public float healthMultiplier = 10;
    public float curHealth;
    public float maxHealth;
    public List<MeshRenderer> coreBlocks;
    public int curTeam;

    [Header("Avoidance Settings")] 
    public float avoidanceRange = 10;
    
    public float avoidanceFalloff;
    public float baseAvoidanceWeight;
    public float isStaleThreshold = 0.1f;
    public float bonusAvoidanceWeight = 0.5f;
    public float bonusAvoidanceDurationMultiplier = 1f;
    public float reverseDuration = 3;
    public float avoidAngle = 30;
    public float droneAheadDistance = 5;
    
    public Vector3 targetDestination;
    Vector3 directionToTarget;
    bool droneAhead;
    float curStoppingDistance;
    
    float curStaleAvoidanceWeight;

    float reverseTime;
    
    List<WheelController> wheelControllers = new List<WheelController>();
    List<HingeController> hingeControllers = new List<HingeController>();
    List<TurretRangeIndicator> rangeIndicators = new List<TurretRangeIndicator>();
    DistanceTracker distanceTracker;
    
    [HideInInspector] public List<MotorBlock> motors = new List<MotorBlock>();
    [HideInInspector] public Rigidbody rb;
    [HideInInspector] public float boundingSphereRadius;
    [HideInInspector] public float velocity;

    MovementState currentMovementState;
    MovementState oldMoveState;
    
    enum MovementState
    {
        Accelerating, Braking, Reversing
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        distanceTracker = GetComponent<DistanceTracker>();
    }

    void Start()
    {
        //Deploy(autoDeploy);
    }

    void Update()
    {
        if (targetDestination == Vector3.zero)
            return;

        velocity = rb.velocity.magnitude;

        currentSteer = Mathf.Clamp(CalculateSteerAmount(targetDestination),-89 * safeTurnAngleMultiplier, 89 * safeTurnAngleMultiplier) ;
        

        if (oldMoveState != MovementState.Reversing && currentMovementState == MovementState.Reversing)
        {
            reverseTime = reverseDuration;
        }
        oldMoveState = currentMovementState;
        
        reverseTime -= Time.deltaTime;

        if (currentMovementState == MovementState.Reversing)
        {
            curStaleAvoidanceWeight = bonusAvoidanceWeight;
        }
        else
        {
            // by the time the car has finished reversing the avoidance decay should be half
            curStaleAvoidanceWeight = Mathf.MoveTowards(curStaleAvoidanceWeight, 0, Time.deltaTime * 0.5f/(reverseDuration*bonusAvoidanceDurationMultiplier));
        }

        DetectDroneAhead();
        
        UpdateComponents();
    }

    public void Select(bool select)
    {
        outline.enabled = select;

        foreach (var rangeIndicator in rangeIndicators)
        {
            rangeIndicator.Select(select);
        }
    }
    
    public void Deploy(bool deploy) // TODO Remove undeploy functionality as it is never used
    {
        GetComponent<DroneBlock>().Init();
        
        rb.isKinematic = !deploy;
        rb.useGravity = deploy;
        rb.mass = mass;
        curHealth *= healthMultiplier;
        maxHealth = curHealth;

        boundingSphereRadius = Utils.CalculateBoundingSphereRadius(rb);
        
        if (deploy)
        {
            comHeight = rb.worldCenterOfMass.y - GetLowerBound();
            
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineWidth = selectionWidth;
            outline.enabled = false;

            HealthBarManager.Instance.RegisterHealthBar(this);

            // Set the core block to the appropriate team colour
            Color teamColour = MatchManager.Instance.Team(curTeam).colour;
            
            
            foreach (var coreBlock in coreBlocks)
            {
                Renderer rend = coreBlock.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.material.color = teamColour;
                }
            }
            
            
            wheelControllers = GetComponentsInChildren<WheelController>().ToList();
            hingeControllers = GetComponentsInChildren<HingeController>().ToList();
            rangeIndicators = GetComponentsInChildren<TurretRangeIndicator>().ToList();
            
            CalculateWheelbase();
            CalculateTrackWidth();
            
        }
    }

    void CalculateWheelbase()
    {
        Vector3 com = rb.worldCenterOfMass;

        List<Vector3> frontAxelPositions = new List<Vector3>();
        List<Vector3> rearAxlePositions = new List<Vector3>();
        
        foreach (var wheel in wheelControllers)
        {

            if (wheel.transform.position.z > com.z) // forward axle
            {
                frontAxelPositions.Add(wheel.transform.position);
            }
            else
            {
                rearAxlePositions.Add(wheel.transform.position);
            }
            
        }

        Vector3 frontAxel = Utils.CalculateAveragePosition(frontAxelPositions);
        Vector3 rearAxel = Utils.CalculateAveragePosition(rearAxlePositions);
        wheelBase = frontAxel.z - rearAxel.z;
    }

    void CalculateTrackWidth()
    {
        Vector3 com = rb.worldCenterOfMass;

        List<Vector3> leftSideWheels = new List<Vector3>();
        List<Vector3> rightSideWheels = new List<Vector3>();
        
        foreach (var wheel in wheelControllers)
        {

            if (wheel.transform.position.x > com.x) // right side
            {
                rightSideWheels.Add(wheel.transform.position);
            }
            else
            {
                leftSideWheels.Add(wheel.transform.position);
            }
            
        }

        Vector3 leftTrack = Utils.CalculateAveragePosition(leftSideWheels);
        Vector3 rightTrack = Utils.CalculateAveragePosition(rightSideWheels);
        trackWidth = rightTrack.x - leftTrack.x;
    }

    void UpdateComponents()
    {
        float turnLimit = 45;
        
        // TODO Investigate this and see if the first condition is necessary
        if (Mathf.Abs(currentSteer) > CalculateMaxSafeSteeringAngle())
        {
            //Debug.Log("Max safe steering angle exceeded current steer: " +Mathf.Abs(currentSteer) + " Current max: " +  CalculateMaxSafeSteeringAngle());
            SetWheelParameters(0); // cornering speed
        }
        else
        {
            SetWheelParameters();
        }

        foreach (HingeController hingeController in hingeControllers)
        {
            if (currentMovementState == MovementState.Reversing)
            {
                hingeController.SetSteerRot(-currentSteer);
            }
            else
            {
                hingeController.SetSteerRot(currentSteer);
            }
        }
    }

    void SetWheelParameters(float targetVelocity = Mathf.Infinity)
    {
        float destinationDistance = Vector3.Distance(transform.position, targetDestination);

        float wheelTorque = motorTorque / wheelControllers.Count;

        curStoppingDistance = CalculateStoppingDistance(mass, 1, motorTorque, rb.velocity.magnitude);
        curStoppingDistance += boundingSphereRadius;
        //curStoppingDistance *= 1;


        if (destinationDistance < curStoppingDistance)
        {
            currentMovementState = MovementState.Braking;
        }
        else if (droneAhead && distanceTracker.totalDistance < isStaleThreshold || reverseTime >0 ) // we arent able to go forward so try reversing
        {
            currentMovementState = MovementState.Reversing;
        }
        else if(droneAhead) // drone is ahead of us
        {
            currentMovementState = MovementState.Braking;
        }
        else
        {
            currentMovementState = MovementState.Accelerating;
        }

        foreach (WheelController wheelController in wheelControllers)
        {
            switch (currentMovementState)
            {
                case MovementState.Accelerating:
                    wheelController.SetForwardTorque(wheelTorque);
                    wheelController.SetTargetVelocity(targetVelocity);
                    break;
                case MovementState.Braking:
                    wheelController.SetTargetVelocity(0);
                    break;
                case MovementState.Reversing:
                    
                    wheelController.SetForwardTorque(wheelTorque);
                    wheelController.SetTargetVelocity(-targetVelocity);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    float GetLowerBound()
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found on GameObject or its children.");
            return gameObject.transform.position.y;
        }

        float minY = float.MaxValue;
        foreach (Collider collider in colliders)
        {
            // Get the minimum Y of the collider's bounds
            float colliderMinY = collider.bounds.min.y;
            if (colliderMinY < minY)
            {
                minY = colliderMinY;
            }
        }

        return minY;
    }

    public void TakeDamage(float damage)
    {
        curHealth -= damage;

        if (curHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetDestination(Transform target)
    {
        targetDestination = target.position;
    }
    
    public void SetDestination(Vector3 dest)
    {
        targetDestination = dest;
        distanceTracker.ResetTracker();
    }

    float CalculateMaxSafeSteeringAngle()
    {
        float gravity = 9.81f;
        float numerator = wheelBase * trackWidth * gravity;
        float denomenator = 2 * (comHeight) * (velocity * velocity);

        //numerator *= 0.5f;
        // Calculate the turn angle in radians
        float turnAngleRadians = Mathf.Atan(numerator/ denomenator);

        // Convert the angle to degrees for easier interpretation (optional)
        float turnAngleDegrees = turnAngleRadians * Mathf.Rad2Deg;

        //Debug.Log("Max safe turn angle" + turnAngleDegrees);
        
        return turnAngleDegrees * safeTurnAngleMultiplier;
    }
    

    float CalculateStoppingDistance(float mass, float wheelRadius, float decelerationTorque, float initialSpeed)
    {
        // Calculate the deceleration force using torque and wheel radius
        float decelerationForce = decelerationTorque / wheelRadius;
    
        // Calculate the deceleration (acceleration in the opposite direction)
        float deceleration = decelerationForce / mass;
    
        // Use the kinematic formula to calculate stopping distance
        // s = (v^2) / (2 * a)
        float stoppingDistance = (initialSpeed * initialSpeed) / (2 * deceleration);
    
        return stoppingDistance;
    }
    
    public Vector3 GetSteering(Vector3 goalDirection, float selfRadius, List<(Vector3 position, float radius)> obstacles)
    {
        // Normalize the goal direction
        goalDirection.Normalize();

        // Initialize the avoidance vector
        Vector3 avoidanceVector = Vector3.zero;

        foreach (var obstacle in obstacles)
        {
            Vector3 obstacleDirection = obstacle.position - transform.position;
            float distanceToObstacle = obstacleDirection.magnitude;

            float combinedRadius = selfRadius + obstacle.radius;

            //float weight = (combinedRadius) / Mathf.Pow(distanceToObstacle,(avoidanceFalloff));
            float weight = 1 - Mathf.InverseLerp(combinedRadius, avoidanceRange, distanceToObstacle);
            
            avoidanceVector -= obstacleDirection.normalized * weight;
        }

        float avoidanceWeight = Mathf.Clamp01(avoidanceVector.magnitude);

        avoidanceVector = avoidanceVector.normalized * avoidanceWeight;

        avoidanceVector *= (baseAvoidanceWeight + curStaleAvoidanceWeight);
        
        // Combine goal direction with avoidance
        Vector3 finalDirection = goalDirection + avoidanceVector;

        // Normalize the result to ensure smooth movement
        return finalDirection.normalized;
    }
    


    float CalculateSteerAmount(Vector3 targetPos)
    {
        directionToTarget = CalculateTargetDirection(targetPos);

        // obstacle avoidance
        
        List<DroneController> drones = FindObjectsOfType<DroneController>().ToList();
        List<(Vector3, float)> obstacles = new List<(Vector3, float)>();
        foreach (var drone in drones)
        {
            if (drone == this)
                continue;
            
            obstacles.Add((drone.transform.position, drone.boundingSphereRadius));
        }
        
        directionToTarget = GetSteering(directionToTarget, boundingSphereRadius, obstacles);
        
        
        Vector3 forward = transform.forward; // The drone's forward direction
        // Calculate the angle between the drone's forward direction and the direction to the target
        float angle = Vector3.SignedAngle(forward, directionToTarget, Vector3.up);

        //angle = ModifySteeringToAvoidCollision(angle);
        

        
        // Convert the angle to a steer value between -1 (hard left) and 1 (hard right)
        float steerAmount = Mathf.Clamp(angle / 90f, -1f, 1f);
        
        return angle * steerMultiplier;
    }

    Vector3 CalculateTargetDirection(Vector3 destination)
    {
        Vector3 origin = transform.position;
        return (destination - origin).normalized; // Direction to the target, normalized
    }

    void DetectDroneAhead()
    {
        droneAhead = false;
        
        //TODO this
        
        float avoidDistance = curStoppingDistance;
        avoidDistance += boundingSphereRadius;
        avoidDistance += droneAheadDistance;

        Vector3 originPoint = transform.position;

        //originPoint -= transform.forward * 10;
    

        List<DroneController> drones = FindObjectsOfType<DroneController>().ToList();

        foreach (var drone in drones)
        {
            if(drone == this)
                continue;
            
            if (Vector3.Distance(transform.position, drone.transform.position) > avoidDistance)
            {
                continue;
            }
            
            
            Vector3 directionToDrone = drone.transform.position - originPoint;
            

            float dot = Vector3.Dot(transform.forward, directionToDrone.normalized);
            dot = (dot + 1) / 2;
            float dotAngle = (1-dot) * 180;

            Debug.Log("DRONE IN DISTANCE ANGLE IS " + dotAngle);
            
            if (dotAngle < avoidAngle)
            {
                droneAhead = true;
            }


        }
    }
    
    float ModifySteeringToAvoidCollision(float steer)
    {
        float newSteer = steer; // Start with the original steer direction
        float avoidDistance = 1000;

        // Convert the steer angle (in degrees) to a direction vector
        Vector3 steerDirection = Quaternion.Euler(0, steer, 0) * Vector3.forward;

        Vector3 originPoint = transform.position;

        List<DroneController> drones = FindObjectsOfType<DroneController>().ToList();

        foreach (var drone in drones)
        {
            if (drone == this)
                continue;

            // Skip drones that are too far away
            if (Vector3.Distance(transform.position, drone.transform.position) > avoidDistance)
                continue;

            // Calculate direction to the drone
            Vector3 directionToDrone = drone.transform.position - originPoint;

            // Project the drone's position onto the forward vector to determine if it's in front
            float dot = Vector3.Dot(transform.forward, directionToDrone.normalized);
            if (dot < 0) // Drone is behind; skip it
                continue;

            // Check if the drone is within a collision danger zone
            float angleToDrone = Vector3.SignedAngle(transform.forward, directionToDrone, Vector3.up);

            // If the drone is too close to the current steer direction, modify the steer
            float dangerZoneAngle = avoidAngle; // Angle in degrees to define a "danger zone"
            if (Mathf.Abs(angleToDrone) < dangerZoneAngle)
            {
                // Avoid the drone by steering slightly away
                if (angleToDrone > 0)
                    newSteer -= (dangerZoneAngle - angleToDrone); // Steer left
                else
                    newSteer += (dangerZoneAngle + angleToDrone); // Steer right
            }
        }

        // Clamp the new steer angle to avoid oversteering
        newSteer = Mathf.Clamp(newSteer, -45f, 45f); // Adjust limits as needed

        return newSteer;
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(rb.worldCenterOfMass, 0.25f);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rb.worldCenterOfMass, boundingSphereRadius);
    }

}
