using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Rotation Speeds (degrees per second)")]
    public float rotationSpeedX = 0f;
    public float rotationSpeedY = 0f;
    public float rotationSpeedZ = 0f;

    void Update()
    {
        // Calculate the rotation for this frame
        float rotationX = rotationSpeedX * Time.deltaTime;
        float rotationY = rotationSpeedY * Time.deltaTime;
        float rotationZ = rotationSpeedZ * Time.deltaTime;

        // Apply the rotation to the transform's local axes
        transform.Rotate(rotationX, rotationY, rotationZ, Space.Self);
    }
}