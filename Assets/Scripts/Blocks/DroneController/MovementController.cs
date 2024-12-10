using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class MovementController
{
    [Header("Steering Settings")]
    public float safeTurnAngleMultiplier = 1f;
    public float steerMultiplier = 1f;

    [Header("Motor Settings")]
    public float motorTorque;
    public float mass;
    public float trackWidth;
    public float wheelBase;
    public float comHeight;

    [Header("Avoidance Settings")]
    public float avoidanceRange = 10f;
    public float avoidanceFalloff = 1f;
    public float baseAvoidanceWeight = 1f;
    public float bonusAvoidanceWeight = 0.5f;
    public float reverseDuration = 3f;
    public float droneAheadDistance = 5f;


    [HideInInspector] public float movementEnergyCost;
    [HideInInspector] public float currentSteer;
    [HideInInspector] public bool droneAhead;
    [HideInInspector] public float velocity;
    DroneController controller;

    private Transform _transform;
    private Rigidbody _rb;

    private List<WheelController> _wheelControllers;
    private List<HingeController> _hingeControllers;
    
    private float _curStoppingDistance;
    private float _curStaleAvoidanceWeight;
    private float _reverseTime;

    public MovementState _currentMovementState;
    private MovementState _oldMoveState;

    public enum MovementState
    {
        Accelerating,
        Braking,
        Reversing
    }

    public void Initialize(Rigidbody rb, Transform transform, DroneController controller)
    {
        this.controller = controller;
        _rb = rb;
        _transform = transform;
    }

    public void InitializeComponents()
    {
        _wheelControllers = _transform.GetComponentsInChildren<WheelController>().ToList();
        _hingeControllers = _transform.GetComponentsInChildren<HingeController>().ToList();

        CalculateComHeight();
        CalculateWheelbase();
        CalculateTrackWidth();
    }

    public void UpdateMovement(Vector3 targetDestination, float boundingSphereRadius)
    {
        velocity = _rb.velocity.magnitude;

        Vector3 directionToTarget = CalculateTargetDirection(targetDestination);
        directionToTarget = GetSteering(directionToTarget, boundingSphereRadius);

        float yawAngleError = Vector3.SignedAngle(_transform.forward, directionToTarget, Vector3.up);
        currentSteer = Mathf.Clamp(yawAngleError * steerMultiplier, -89 * safeTurnAngleMultiplier, 89 * safeTurnAngleMultiplier);

        float destinationDistance = Vector3.Distance(_transform.position, targetDestination);
        
        UpdateMovementState(velocity, boundingSphereRadius, destinationDistance);
        UpdateComponents();
    }
    

    private void UpdateMovementState(float velocity, float boundingSphereRadius, float destinationDistance)
    {
        
        if (_oldMoveState != MovementState.Reversing && _currentMovementState == MovementState.Reversing)
        {
            _reverseTime = reverseDuration;
        }
        _oldMoveState = _currentMovementState;

        _reverseTime -= Time.deltaTime;

        if (_currentMovementState == MovementState.Reversing)
        {
            _curStaleAvoidanceWeight = bonusAvoidanceWeight;
        }
        else
        {
            _curStaleAvoidanceWeight = Mathf.MoveTowards(_curStaleAvoidanceWeight, 0,
                Time.deltaTime * 0.5f / (reverseDuration));
        }

        float stoppingDistance = CalculateStoppingDistance(mass, 1, motorTorque, velocity);
        stoppingDistance += boundingSphereRadius;

        if (destinationDistance < stoppingDistance || Mathf.Abs(currentSteer) > CalculateMaxSafeSteeringAngle())
        {
            /*if(Mathf.Abs(currentSteer) > CalculateMaxSafeSteeringAngle())
                Debug.Log("BREAKING FOR STEERING");*/
            _currentMovementState = MovementState.Braking;
        }
        else if (droneAhead && _reverseTime > 0)
        {
            _currentMovementState = MovementState.Reversing;
        }
        else
        {
            _currentMovementState = MovementState.Accelerating;
        }
    }

    private void UpdateComponents()
    {
        if(_hingeControllers == null)
            return;
        
        foreach (HingeController hingeController in _hingeControllers)
        {
            if (_currentMovementState == MovementState.Reversing)
            {
                hingeController.SetSteerRot(-currentSteer);
            }
            else
            {
                hingeController.SetSteerRot(currentSteer);
            }
        }
        
        float wheelTorque = motorTorque / _wheelControllers.Count;
        float targetVelocity = Mathf.Infinity;
        /*foreach (WheelController wheelController in _wheelControllers)
        {
            switch (_currentMovementState)
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
        }*/
        
        float energyCost = movementEnergyCost * Time.deltaTime;

        float torqueMultiplier = 0;

        if (controller.energy.CanAfford(energyCost))
        {
            torqueMultiplier = 1;
        }

        
        switch (_currentMovementState)
        {
            case MovementState.Accelerating:
                
                controller.energy.DeductEnergy(energyCost);
                
                foreach (WheelController wheelController in _wheelControllers)
                {
                    wheelController.SetForwardTorque(wheelTorque * torqueMultiplier);
                    wheelController.SetTargetVelocity(targetVelocity);
                }
                break;
            case MovementState.Braking:
                foreach (WheelController wheelController in _wheelControllers)
                {
                    wheelController.SetForwardTorque(wheelTorque);
                    wheelController.SetTargetVelocity(0);
                }
                break;
            case MovementState.Reversing:
                
                controller.energy.DeductEnergy(energyCost);
                
                foreach (WheelController wheelController in _wheelControllers)
                {
                    wheelController.SetForwardTorque(wheelTorque * torqueMultiplier);
                    wheelController.SetTargetVelocity(-targetVelocity);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Vector3 CalculateTargetDirection(Vector3 destination)
    {
        Vector3 origin = _transform.position;
        return (destination - origin).normalized;
    }

    private Vector3 GetSteering(Vector3 goalDirection, float boundingSphereRadius)
    {
        Vector3 avoidanceVector = Vector3.zero;

        foreach (var drone in Object.FindObjectsOfType<DroneController>())
        {
            if (drone == null || drone.transform == _transform) continue;

            Vector3 obstacleDirection = drone.transform.position - _transform.position;
            float distanceToObstacle = obstacleDirection.magnitude;

            float combinedRadius = boundingSphereRadius + drone.boundingSphereRadius;
            float weight = Mathf.Clamp01(1 - Mathf.InverseLerp(combinedRadius, avoidanceRange, distanceToObstacle));
            avoidanceVector -= obstacleDirection.normalized * weight;
        }

        avoidanceVector *= (baseAvoidanceWeight + _curStaleAvoidanceWeight);
        return (goalDirection + avoidanceVector).normalized;
    }

    private void CalculateWheelbase()
    {
        Vector3 com = _rb.worldCenterOfMass;

        List<Vector3> frontAxlePositions = new List<Vector3>();
        List<Vector3> rearAxlePositions = new List<Vector3>();

        foreach (var wheel in _wheelControllers)
        {
            if (wheel.transform.position.z > com.z)
            {
                frontAxlePositions.Add(wheel.transform.position);
            }
            else
            {
                rearAxlePositions.Add(wheel.transform.position);
            }
        }

        Vector3 frontAxle = Utils.CalculateAveragePosition(frontAxlePositions);
        Vector3 rearAxle = Utils.CalculateAveragePosition(rearAxlePositions);
        wheelBase = frontAxle.z - rearAxle.z;
    }

    private void CalculateTrackWidth()
    {
        Vector3 com = _rb.worldCenterOfMass;

        List<Vector3> leftSideWheels = new List<Vector3>();
        List<Vector3> rightSideWheels = new List<Vector3>();

        foreach (var wheel in _wheelControllers)
        {
            if (wheel.transform.position.x > com.x)
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

    void CalculateComHeight()
    {
        comHeight = _rb.worldCenterOfMass.y - GetLowerBound();
    }

    private float GetLowerBound()
    {
        Collider[] colliders = _transform.GetComponentsInChildren<Collider>();
        return colliders.Min(collider => collider.bounds.min.y);
    }

    private float CalculateStoppingDistance(float mass, float wheelRadius, float decelerationTorque, float initialSpeed)
    {
        float deceleration = decelerationTorque / (mass * wheelRadius);
        return (initialSpeed * initialSpeed) / (2 * deceleration);
    }
    
    public float CalculateMaxSafeSteeringAngle()
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
}
