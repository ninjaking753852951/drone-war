using System;
using UnityEngine;
using UnityEngine.Serialization;
public class TurretMountSingleAxis : MovingDroneBlockBase, IProxyDeploy
{
    public ControlType controlType;
    
    public enum ControlType
    {
        Yaw, Pitch
    }

    public Transform body;
    
    public HingeJoint joint;
    
    public float aimForce = 100;
    public float maxTurnSpeed = 10;
    public float nearTargetSmoothing = 1;
    
    [Header("RUNTIME VARIABLES")]
    public float targetAngle = 1;

    
    void Update()
    {
        SetYawAngle(targetAngle);
    }
    
    
    public override void Deploy()
    {
        base.Deploy();
        
        
        
        body.transform.parent = transform.parent;
        
        InitJoint();   
        return;
        
        Rigidbody parentRb = Utils.FindParentRigidbody(transform, rb);
        if (parentRb != null)
        {
            Debug.Log(parentRb.gameObject.name);
            //joint.connectedBody = parentRb;
            InitJoint();   
        }
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
        float currentAngle = joint.angle % 360;
        float normalizedTargetAngle = targetYawAngle % 360;
        
        float angleError = Mathf.DeltaAngle(currentAngle, normalizedTargetAngle);
        
        angleError = Mathf.Clamp(angleError / nearTargetSmoothing, -1, 1);
        

        
        JointMotor jointMotor = joint.motor;
        //jointMotor.targetVelocity = angleError * maxTurnSpeed; // Adjust multiplier for speed control

        float targetVelocity = angleError * maxTurnSpeed;
        
        if(!float.IsNaN(targetVelocity))
            jointMotor.targetVelocity = targetVelocity;

        joint.motor = jointMotor;
    }

    void InitJoint()
    {

        
        JointMotor jointMotor = joint.motor;
        jointMotor.force = aimForce;
        joint.motor = jointMotor;
        joint.useMotor = true;
        
        Rigidbody parentRb = Utils.FindParentRigidbody(transform, rb);
        joint.connectedBody = parentRb;
    }

    public void ProxyDeploy()
    {
        body.transform.parent = transform.parent;
    }
}
