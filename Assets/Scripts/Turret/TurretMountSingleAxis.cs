using System;
using UnityEngine;
using UnityEngine.Serialization;
public class TurretMountSingleAxis : MonoBehaviour, IProxyDeploy
{

    public ControlType controlType;
    
    public enum ControlType
    {
        Yaw, Pitch
    }

    public Transform body;
    
    public HingeJoint joint;

    public float readyToFirePositionThreshold = 5;
    public float readyToFireDelta = 0.1f;
    public float aimForce = 100;
    public float maxTurnSpeed = 10;
    public float nearTargetSmoothing = 1;
    
    TurretCoreController turret;

    [Header("RUNTIME VARIABLES")]
    public float targetAngle;
    
    float angleDelta;
    float anglePos;
    
    
    [HideInInspector]
    public Rigidbody rb;

    Vector3 targetPosition;

    void Awake()
    {
        rb = joint.GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public void Init()
    {
        body.transform.parent = transform.parent;
    }
    
    // Start is called before the first frame update
    void Start()
    {

    }

    void FixedUpdate()
    {
        angleDelta = anglePos - joint.angle;
        anglePos = joint.angle;
    }

    void Update()
    {
        SetYawAngle(targetAngle);
    }


    public void Deploy(TurretCoreController controller)
    {
        turret = controller;

        rb.isKinematic = false;
        rb.useGravity = true;

        joint.connectedBody = Utils.FindParentRigidbody(transform, rb);
        InitJointMotors();
    }
    
    public void UpdateTurretAngles(float yaw, float pitch)
    {
        switch (controlType)
        {

            case ControlType.Yaw:
                targetAngle = yaw;
                break;
            case ControlType.Pitch:
                targetAngle = pitch;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    void SetYawAngle(float targetYawAngle)
    {
        float currentAngle = (joint.angle % 360 + 360) % 360;
        float normalizedTargetAngle = (targetYawAngle % 360 + 360) % 360;
        
        float angleError = Mathf.DeltaAngle(currentAngle, normalizedTargetAngle);
        
        angleError = Mathf.Clamp(angleError / nearTargetSmoothing, -1, 1);
        
        JointMotor jointMotor = joint.motor;
        jointMotor.targetVelocity = angleError * maxTurnSpeed; // Adjust multiplier for speed control
        joint.motor = jointMotor;
    }

    void InitJointMotors()
    {
        InitYawJoint();
    }

    void InitYawJoint()
    {
        JointMotor yawMotor = joint.motor;
        yawMotor.force = aimForce;
        joint.motor = yawMotor;
        joint.useMotor = true;
    }


    public bool ReadyToFire()
    {
        
        float maxAngle = GetMaxAllowableAngle(transform.position, targetPosition, readyToFirePositionThreshold);
        
        float angleError = joint.angle - targetAngle;
        


        return angleError < maxAngle && angleDelta < readyToFireDelta;
    }
    
    float GetMaxAllowableAngle(Vector3 P1, Vector3 P2, float tolerance)
    {
        return Mathf.Infinity;
        float distance = Vector3.Distance(P1, P2);
        if (distance <= tolerance) return 180f; // Covers all directions
        return Mathf.Asin(tolerance / distance) * Mathf.Rad2Deg;
    }

    public void ProxyDeploy()
    {
        body.transform.parent = transform.parent;
    }
}
