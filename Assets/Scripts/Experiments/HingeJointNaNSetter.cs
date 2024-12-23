using UnityEngine;

public class HingeJointNaNSetter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HingeJoint joint = GetComponent<HingeJoint>();
        JointMotor motor = joint.motor;
        motor.targetVelocity = float.NaN;
        joint.motor = motor;
        joint.useMotor = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
