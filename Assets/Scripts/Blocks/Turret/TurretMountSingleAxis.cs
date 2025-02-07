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

    public PhysJointPhysBlock block { private set; get; }
    bool isDeployed = false;


    void Awake()
    {
        block = GetComponent<PhysJointPhysBlock>();
        block.onBuildFinalized.AddListener(Deploy);
    }
    
    void Update()
    {
        if(joint != null)
            SetYawAngle(targetAngle);
    }
    
    
    public override void Deploy()
    {
        joint = (HingeJoint)block.joint;
        if (joint == null)
        {
            enabled = false;
            return;
        }
            
        JointMotor jointMotor = joint.motor;
        jointMotor.force = aimForce;
        joint.motor = jointMotor;
    }
    
    public void UpdateTurretAngles(float yaw, float pitch)
    {
        //Debug.Log("UPDATE MOUNTS TURRET");
        
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
        Debug.DrawRay(transform. position, Quaternion.Euler(0,targetYawAngle,0) * transform.forward, Color.green);
        
        float currentAngle = joint.angle % 360;
        if (float.IsNaN(joint.angle))
            currentAngle = 0;


        
        float normalizedTargetAngle = targetYawAngle % 360;
        
        float angleError = Mathf.DeltaAngle(currentAngle, -normalizedTargetAngle);
        
        angleError = Mathf.Clamp(angleError / nearTargetSmoothing, -1, 1);

        
        JointMotor jointMotor = joint.motor;
        //jointMotor.targetVelocity = angleError * maxTurnSpeed; // Adjust multiplier for speed control

        float targetVelocity = angleError * maxTurnSpeed;

        if (!float.IsNaN(targetVelocity))
        {
            jointMotor.targetVelocity = targetVelocity;   

            //jointMotor.targetVelocity = Mathf.Sin(Time.time/2) * 100;   
        }
        else
        {
            //Debug.Log("AIM IS NAN");
        }

        joint.motor = jointMotor;
    }

    /*void InitJoint()
    {
        JointMotor jointMotor = joint.motor;
        jointMotor.force = aimForce;
        joint.motor = jointMotor;
        joint.useMotor = true;
        
        Rigidbody parentRb = Utils.FindParentRigidbody(transform, rb);
        joint.connectedBody = parentRb;
    }*/

    public void ProxyDeploy()
    {
        body.transform.parent = transform.parent;
    }
}
