using System;
using System.Collections.Generic;
using UnityEngine;

public class TankTrack : MonoBehaviour
{
    public List<Rigidbody> neighborWheels = new List<Rigidbody>();

    SphereCollider primaryWheel;
    
    public float linearSpeed;

    void Awake()
    {
        if (neighborWheels == null || neighborWheels.Count == 0)
            return;
        
        primaryWheel = neighborWheels[0].GetComponent<SphereCollider>();
    }

    void FixedUpdate()
    {
        if (neighborWheels == null || neighborWheels.Count == 0)
            return;

        // Calculate the average angular velocity
        Vector3 averageAngularVelocity = Vector3.zero;

        foreach (Rigidbody wheel in neighborWheels)
        {
            if (wheel != null)
            {
                averageAngularVelocity += wheel.angularVelocity;
            }
        }

        averageAngularVelocity /= neighborWheels.Count; // Divide to get the average

        linearSpeed = averageAngularVelocity.y * primaryWheel.radius * 2;
        
        // Apply the average angular velocity to all wheels
        foreach (Rigidbody wheel in neighborWheels)
        {
            if (wheel != null)
            {
                wheel.angularVelocity = averageAngularVelocity;
            }
        }
    }

}