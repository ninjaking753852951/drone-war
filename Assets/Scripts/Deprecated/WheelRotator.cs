using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(WheelCollider))]
public class WheelRotator : MonoBehaviour
{
    public Transform wheelBody;
    WheelCollider wheel;

    public float spin;

    float torqueDirection;
    
    // Start is called before the first frame update
    void Start()
    {
        wheel = GetComponent<WheelCollider>();
        
        wheel.steerAngle = wheelBody.rotation.eulerAngles.y;

        torqueDirection = CalculateTorqueDirection();

        //Debug.Log(wheel.);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position;
        Quaternion rotation;
        wheel.GetWorldPose(out position, out rotation);

        // Apply position and rotation to the visual wheel model
        //wheelBody.position = position;
        wheelBody.rotation = rotation;

        spin += Time.deltaTime;

        //wheel.steerAngle = spin;

        wheel.motorTorque = spin * torqueDirection;
    }

    float CalculateTorqueDirection()
    {
        Transform origin = transform.root;
        // Direction from a to b
        Vector3 directionToWheelBody = wheelBody.transform.position - origin.position;

        // Calculate the dot product with a's right direction
        float dotProduct = Vector3.Dot(directionToWheelBody.normalized, origin.right);

        // If the dot product is positive, b is on the right side of a
        return dotProduct > 0 ? 1 : -1;
        
    }
}
